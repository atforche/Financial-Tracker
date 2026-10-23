import "server-only";

import createClient, { type Client, type Middleware } from "openapi-fetch";
import getCallbackPath, {
  currentPathHeader,
} from "@/framework/auth/callbackPath";
import getGoogleIdToken from "@/framework/data/getGoogleIdToken";
import { headers } from "next/headers";
import type { paths } from "@/framework/data/api";
import { redirect } from "next/navigation";

/**
 * Sends the user to end their session and sign in again, returning them to the page they were on
 * afterwards. Server actions and server components both turn this into a browser navigation. The proxy
 * records the requested page, which for a server action is the page it was called from; any external
 * callback is discarded.
 */
const redirectToLogin = async function (): Promise<never> {
  const callbackUrl = getCallbackPath((await headers()).get(currentPathHeader));
  redirect(`/session-expired?callbackUrl=${encodeURIComponent(callbackUrl)}`);
};

/**
 * Sends the user to log in again when the backend rejects their token.
 */
const redirectOnUnauthorized: Middleware = {
  async onResponse({ response }) {
    if (response.status === 401) {
      await redirectToLogin();
    }
    return undefined;
  },
};

/**
 * Creates an API client for the current authenticated user. Redirects to the login page when the session
 * no longer has a usable token.
 */
const createApiClient = async function (): Promise<Client<paths>> {
  const apiUrl = process.env["API_URL"];
  if (typeof apiUrl === "undefined" || apiUrl.trim() === "") {
    throw new Error("API_URL must be configured before using the API client.");
  }

  const idToken = await getGoogleIdToken();
  if (idToken === null || idToken === "") {
    return redirectToLogin();
  }

  const client = createClient<paths>({
    baseUrl: apiUrl,
    headers: { Authorization: `Bearer ${idToken}` },
  });
  client.use(redirectOnUnauthorized);
  return client;
};

export default createApiClient;
