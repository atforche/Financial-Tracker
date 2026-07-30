import Google from "next-auth/providers/google";
import NextAuth from "next-auth";

declare module "next-auth" {
  interface Session {
    idToken?: string;
  }
}

declare module "@auth/core/jwt" {
  // eslint-disable-next-line @typescript-eslint/naming-convention
  interface JWT {
    idToken?: string;
  }
}

const google = Google;
const nextAuth = NextAuth;
const allowedGoogleSubjects = (process.env["GOOGLE_ALLOWED_SUBJECTS"] ?? "")
  .split(",")
  .map((subject) => subject.trim())
  .filter((subject) => subject !== "");

/**
 * Configures Google OpenID Connect login and retains the ID token for backend calls.
 */
const { auth, handlers, signIn, signOut } = nextAuth({
  providers: [
    google({
      clientId: process.env["GOOGLE_CLIENT_ID"] ?? "",
      clientSecret: process.env["GOOGLE_CLIENT_SECRET"] ?? "",
    }),
  ],
  callbacks: {
    signIn({ profile }) {
      return (
        typeof profile?.sub === "string" &&
        allowedGoogleSubjects.includes(profile.sub)
      );
    },
    authorized({ auth: session }) {
      return session !== null;
    },
    jwt({ token, account }) {
      if (typeof account?.id_token === "string") {
        const tokenWithIdToken = token as typeof token & { idToken?: string };
        tokenWithIdToken.idToken = account.id_token;
      }
      return token;
    },
  },
});

export { auth, handlers, signIn, signOut };
