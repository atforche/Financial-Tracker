import {
  getIdTokenExpiration,
  isIdTokenExpired,
} from "@/framework/auth/idTokenExpiration";
import Credentials from "next-auth/providers/credentials";
import Google from "next-auth/providers/google";
import NextAuth from "next-auth";

declare module "@auth/core/jwt" {
  // eslint-disable-next-line @typescript-eslint/naming-convention
  interface JWT {
    idToken?: string;
    idTokenExpiresAt?: number;
  }
}

const google = Google;
const credentials = Credentials;
const nextAuth = NextAuth;
const authenticationMode = process.env["AUTH_MODE"] ?? "google";
const usesDevelopmentAuthentication = authenticationMode === "development";
const usesGoogleAuthentication = authenticationMode === "google";
if (!usesDevelopmentAuthentication && !usesGoogleAuthentication) {
  throw new Error("AUTH_MODE must be either 'development' or 'google'.");
}
const allowedGoogleSubjects = (process.env["GOOGLE_ALLOWED_SUBJECTS"] ?? "")
  .split(",")
  .map((subject) => subject.trim())
  .filter((subject) => subject !== "");
const developmentSubject = process.env["DEVELOPMENT_AUTH_SUBJECT"] ?? "";
if (usesDevelopmentAuthentication && developmentSubject.trim() === "") {
  throw new Error(
    "DEVELOPMENT_AUTH_SUBJECT must be configured when AUTH_MODE=development.",
  );
}
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
        credentials({
          id: "development",
          name: "Local development",
          credentials: {},
          authorize() {
            return {
              id: developmentSubject,
              name: "Local developer",
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
    signIn: "/login",
  },
  callbacks: {
    signIn({ account, profile, user }) {
      if (account?.provider === "development") {
        return user.id === developmentSubject;
      }

      const subject = typeof profile?.sub === "string" ? profile.sub : null;
      const isAllowed =
        subject !== null && allowedGoogleSubjects.includes(subject);

      if (!isAllowed && subject !== null) {
        // eslint-disable-next-line no-console
        console.warn("Rejected Google sign-in for unapproved subject", {
          subject,
        });
      }

      return isAllowed;
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

export { authenticationProvider, auth, handlers, signIn, signOut };
