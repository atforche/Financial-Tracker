import Google from "next-auth/providers/google";
import NextAuth from "next-auth";

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
  pages: {
    signIn: "/login",
  },
  callbacks: {
    signIn({ profile }) {
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
