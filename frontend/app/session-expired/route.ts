import { type NextRequest, NextResponse } from "next/server";
import getCallbackPath from "@/framework/auth/callbackPath";
import getGoogleIdToken from "@/framework/data/getGoogleIdToken";
import { signOut } from "@/auth";

/**
 * Returns whether the backend still accepts the provided token.
 */
const isTokenAccepted = async function (idToken: string): Promise<boolean> {
  const apiUrl = process.env["API_URL"];
  if (typeof apiUrl === "undefined" || apiUrl.trim() === "") {
    throw new Error("API_URL must be configured before checking a session.");
  }
  const response = await fetch(new URL("/users/me", apiUrl).toString(), {
    headers: { Authorization: `Bearer ${idToken}` },
    cache: "no-store",
  });
  return response.status !== 401;
};

/**
 * Ends a session whose token the backend no longer accepts and sends the user to sign in again. Pages
 * cannot clear the session cookie themselves, so they redirect here; otherwise the login page would see
 * the session and send the user straight back. The token is checked first, so a link to this route cannot
 * sign out a user whose session is still valid.
 */
const GET = async function (request: NextRequest): Promise<NextResponse> {
  const callbackPath = getCallbackPath(
    request.nextUrl.searchParams.get("callbackUrl"),
  );
  const idToken = await getGoogleIdToken();
  if (idToken !== null && (await isTokenAccepted(idToken))) {
    return NextResponse.redirect(new URL(callbackPath, request.nextUrl));
  }
  return signOut({
    redirectTo: `/login?callbackUrl=${encodeURIComponent(callbackPath)}`,
  });
};

export { GET };
