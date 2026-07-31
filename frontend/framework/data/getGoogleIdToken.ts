import "server-only";

import { getToken } from "next-auth/jwt";
import { headers } from "next/headers";

/**
 * Reads the Google ID token from the encrypted Auth.js session cookie without
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

  return typeof token?.idToken === "string" ? token.idToken : null;
};

export default getGoogleIdToken;
