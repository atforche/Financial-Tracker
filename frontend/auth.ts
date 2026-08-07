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
        }),
      ],
  pages: {
    error: "/login",
    signIn: "/login",
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
      return request.nextUrl.pathname === "/login" || session !== null;
    },
    jwt({ token, account, user }) {
      if (typeof account?.id_token === "string") {
        const tokenWithIdToken = token as typeof token & { idToken?: string };
        tokenWithIdToken.idToken = account.id_token;
        const expiration = getIdTokenExpiration(account.id_token);
        if (expiration !== null) {
          tokenWithIdToken.idTokenExpiresAt = expiration;
        }
      }
      if (account?.provider === "development" && typeof user.id === "string") {
        const tokenWithIdToken = token as typeof token & { idToken?: string };
        tokenWithIdToken.idToken = `development:${user.id}`;
      }

      if (hasExpiredIdToken(token)) {
        return null;
      }

      return token;
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
