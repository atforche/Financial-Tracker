import "server-only";

import { getToken } from "next-auth/jwt";
import { headers } from "next/headers";
import { isIdTokenExpired } from "@/framework/auth/idTokenExpiration";

/**
 * Reads the API access token from the encrypted Auth.js session cookie without
 * exposing it through the browser-visible session response.
 */
const getGoogleIdToken = async function (): Promise<string | null> {
  const secret = process.env["AUTH_SECRET"];
  if (typeof secret === "undefined" || secret.trim() === "") {
    throw new Error(
      "AUTH_SECRET must be configured before using the API client.",
    );
  }

  const authUrl = process.env["AUTH_URL"];
  const secureCookie =
    typeof authUrl === "string" && new URL(authUrl).protocol === "https:";
  const token = await getToken({
    req: { headers: new Headers(await headers()) },
    secret,
    secureCookie,
  });

  if (typeof token?.idToken !== "string") {
    return null;
  }
  if (token.idToken.startsWith("development:")) {
    return token.idToken;
  }

  return isIdTokenExpired(token.idToken, token.idTokenExpiresAt)
    ? null
    : token.idToken;
};

export default getGoogleIdToken;
