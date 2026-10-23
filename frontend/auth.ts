import {
  getIdTokenExpiration,
  isIdTokenExpired,
} from "@/framework/auth/idTokenExpiration";
import Credentials from "next-auth/providers/credentials";
import Google from "next-auth/providers/google";
import NextAuth from "next-auth";
import resolveApplicationUser from "@/framework/auth/resolveApplicationUser";

declare module "@auth/core/jwt" {
  // eslint-disable-next-line @typescript-eslint/naming-convention
  interface JWT {
    idToken?: string;
    idTokenExpiresAt?: number;
    refreshToken?: string;
    signedInAt?: number;
  }
}

interface DevelopmentAuthenticationIdentity {
  label: string;
  subject: string;
}

const google = Google;
const credentialsProvider = Credentials;
const nextAuth = NextAuth;
const authenticationMode = process.env["AUTH_MODE"] ?? "google";
const usesDevelopmentAuthentication = authenticationMode === "development";
const usesGoogleAuthentication = authenticationMode === "google";
if (!usesDevelopmentAuthentication && !usesGoogleAuthentication) {
  throw new Error("AUTH_MODE must be either 'development' or 'google'.");
}
const developmentSubject = process.env["DEVELOPMENT_AUTH_SUBJECT"] ?? "";
if (usesDevelopmentAuthentication && developmentSubject.trim() === "") {
  throw new Error(
    "DEVELOPMENT_AUTH_SUBJECT must be configured when AUTH_MODE=development.",
  );
}
const additionalDevelopmentSubjects = (
  process.env["DEVELOPMENT_AUTH_ADDITIONAL_SUBJECTS"] ??
  (developmentSubject === "local-developer"
    ? "local-standard,local-read-only"
    : "")
)
  .split(",")
  .map((subject) => subject.trim())
  .filter((subject) => subject !== "");
const readOnlyDevelopmentSubjects = new Set(
  (
    process.env["DEVELOPMENT_AUTH_READ_ONLY_SUBJECTS"] ??
    (developmentSubject === "local-developer" ? "local-read-only" : "")
  )
    .split(",")
    .map((subject) => subject.trim())
    .filter((subject) => subject !== ""),
);
const developmentAuthenticationIdentities: readonly DevelopmentAuthenticationIdentity[] =
  [
    {
      label: "Administrator",
      subject: developmentSubject,
    },
    ...additionalDevelopmentSubjects
      .filter((subject) => subject !== developmentSubject)
      .map((subject) => ({
        label: readOnlyDevelopmentSubjects.has(subject)
          ? "Read-only user"
          : "Standard user",
        subject,
      })),
  ];
const developmentSubjects = new Set(
  developmentAuthenticationIdentities.map((identity) => identity.subject),
);
const authenticationProvider = usesDevelopmentAuthentication
  ? "development"
  : "google";

/**
 * How long before the ID token expires that it is refreshed. Server components read the session cookie
 * from the incoming request, so they keep using the previous token for that request; the margin keeps it
 * valid for the backend in the meantime.
 */
const idTokenRefreshMarginSeconds = 5 * 60;

/**
 * Longest a session lasts after sign-in, however often its ID token is refreshed. The refresh token lets
 * a session continue indefinitely, so this bounds how long a copied session cookie remains usable.
 */
const maximumSessionLengthSeconds = 7 * 24 * 60 * 60;

/**
 * Returns whether the provider token should be refreshed before it is used again.
 */
const shouldRefreshIdToken = function (token: {
  idToken?: string;
  idTokenExpiresAt?: number;
}): boolean {
  if (
    typeof token.idToken !== "string" ||
    token.idToken.startsWith("development:")
  ) {
    return false;
  }
  const expiration =
    typeof token.idTokenExpiresAt === "number"
      ? token.idTokenExpiresAt
      : getIdTokenExpiration(token.idToken);
  return (
    expiration === null ||
    expiration - idTokenRefreshMarginSeconds <= Math.floor(Date.now() / 1000)
  );
};

/**
 * Exchanges a Google refresh token for a new ID token, or returns null when Google rejects it.
 */
const refreshGoogleIdToken = async function (
  refreshToken: string,
): Promise<{ idToken: string; refreshToken?: string } | null> {
  try {
    const response = await fetch("https://oauth2.googleapis.com/token", {
      method: "POST",
      body: new URLSearchParams([
        ["client_id", process.env["GOOGLE_CLIENT_ID"] ?? ""],
        ["client_secret", process.env["GOOGLE_CLIENT_SECRET"] ?? ""],
        ["grant_type", "refresh_token"],
        ["refresh_token", refreshToken],
      ]),
    });
    if (!response.ok) {
      return null;
    }
    const body: unknown = await response.json();
    if (typeof body !== "object" || body === null) {
      return null;
    }
    const idToken: unknown = Reflect.get(body, "id_token");
    const newRefreshToken: unknown = Reflect.get(body, "refresh_token");
    if (typeof idToken !== "string") {
      return null;
    }
    return typeof newRefreshToken === "string"
      ? { idToken, refreshToken: newRefreshToken }
      : { idToken };
  } catch {
    return null;
  }
};

/**
 * Revokes a Google refresh token so a copy of the session cookie can no longer obtain new ID tokens.
 * Failures are ignored; signing out still clears the session cookie.
 */
const revokeGoogleRefreshToken = async function (
  refreshToken: string,
): Promise<void> {
  try {
    await fetch("https://oauth2.googleapis.com/revoke", {
      method: "POST",
      body: new URLSearchParams([["token", refreshToken]]),
    });
  } catch {
    // The refresh token remains valid until it is revoked from the Google account.
  }
};

/**
 * Returns whether the provider token is still usable by the backend.
 */
const hasExpiredIdToken = function (token: {
  idToken?: string;
  idTokenExpiresAt?: number;
}): boolean {
  if (
    typeof token.idToken !== "string" ||
    token.idToken.startsWith("development:")
  ) {
    return false;
  }

  const expiration =
    typeof token.idTokenExpiresAt === "number"
      ? token.idTokenExpiresAt
      : getIdTokenExpiration(token.idToken);

  return isIdTokenExpired(token.idToken, expiration);
};

/**
 * Configures Google OpenID Connect login and retains the ID token for backend calls.
 */
const { auth, handlers, signIn, signOut } = nextAuth({
  providers: usesDevelopmentAuthentication
    ? [
        credentialsProvider({
          id: "development",
          name: "Local development",
          credentials: {
            subject: {},
          },
          authorize(submittedCredentials) {
            const { subject } = submittedCredentials;
            if (
              typeof subject !== "string" ||
              !developmentSubjects.has(subject)
            ) {
              return null;
            }
            return {
              id: subject,
              name:
                developmentAuthenticationIdentities.find(
                  (identity) => identity.subject === subject,
                )?.label ?? "Local developer",
            };
          },
        }),
      ]
    : [
        google({
          clientId: process.env["GOOGLE_CLIENT_ID"] ?? "",
          clientSecret: process.env["GOOGLE_CLIENT_SECRET"] ?? "",
          // Offline access returns a refresh token so the session can outlive the one-hour ID token.
          // Google only issues one on consent, so consent is requested at each sign-in.
          authorization: {
            // eslint-disable-next-line camelcase -- Google's parameter name.
            params: { access_type: "offline", prompt: "consent" },
          },
        }),
      ],
  pages: {
    error: "/login",
    signIn: "/login",
  },
  events: {
    async signOut(message) {
      if (
        "token" in message &&
        typeof message.token?.refreshToken === "string"
      ) {
        await revokeGoogleRefreshToken(message.token.refreshToken);
      }
    },
  },
  callbacks: {
    async signIn({ account, user }) {
      const idToken =
        account?.provider === "development"
          ? typeof user.id === "string"
            ? `development:${user.id}`
            : null
          : typeof account?.id_token === "string"
            ? account.id_token
            : null;
      if (idToken === null) {
        return false;
      }

      try {
        return await resolveApplicationUser(idToken);
      } catch {
        // eslint-disable-next-line no-console
        console.error("Application user resolution failed during sign-in.");
        return false;
      }
    },
    authorized({ auth: session, request }) {
      /*
       * A redirect response to a server action reaches the browser as an unreadable action result rather
       * than a navigation, so server actions are let through. They cannot reach the backend without a
       * usable token, and the API client redirects them to the login page itself.
       */
      return (
        request.nextUrl.pathname === "/login" ||
        session !== null ||
        (request.method === "POST" && request.headers.has("next-action"))
      );
    },
    async jwt({ token, account, user }) {
      if (typeof account?.id_token === "string") {
        const tokenWithIdToken = token as typeof token & { idToken?: string };
        tokenWithIdToken.idToken = account.id_token;
        const expiration = getIdTokenExpiration(account.id_token);
        if (expiration !== null) {
          tokenWithIdToken.idTokenExpiresAt = expiration;
        }
        if (typeof account.refresh_token === "string") {
          token.refreshToken = account.refresh_token;
        }
      }
      if (account !== null && typeof account !== "undefined") {
        token.signedInAt = Math.floor(Date.now() / 1000);
      }
      // Sessions from before the maximum length existed start counting from now.
      token.signedInAt ??= Math.floor(Date.now() / 1000);
      if (
        token.signedInAt + maximumSessionLengthSeconds <=
        Math.floor(Date.now() / 1000)
      ) {
        return null;
      }
      if (account?.provider === "development" && typeof user.id === "string") {
        const tokenWithIdToken = token as typeof token & { idToken?: string };
        tokenWithIdToken.idToken = `development:${user.id}`;
      }

      const refreshed =
        shouldRefreshIdToken(token) && typeof token.refreshToken === "string"
          ? await refreshGoogleIdToken(token.refreshToken)
          : null;
      // When the refresh fails, the current token is kept and the expiry check decides whether the session ends.
      const currentToken: typeof token =
        refreshed === null
          ? token
          : {
              ...token,
              idToken: refreshed.idToken,
              ...(typeof refreshed.refreshToken === "string"
                ? { refreshToken: refreshed.refreshToken }
                : {}),
            };
      if (refreshed !== null) {
        const expiration = getIdTokenExpiration(refreshed.idToken);
        if (expiration === null) {
          delete currentToken.idTokenExpiresAt;
        } else {
          currentToken.idTokenExpiresAt = expiration;
        }
      }

      if (hasExpiredIdToken(currentToken)) {
        return null;
      }

      return currentToken;
    },
  },
});

export {
  authenticationProvider,
  auth,
  developmentAuthenticationIdentities,
  handlers,
  signIn,
  signOut,
};
