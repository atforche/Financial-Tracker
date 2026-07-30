import "server-only";

import createClient, { type Client } from "openapi-fetch";
import type { paths } from "@/framework/data/api";

let apiClient: Client<paths> | null = null;

/**
 * Creates or returns the shared API client for making backend requests.
 */
const createApiClient = function (): Client<paths> {
  const apiUrl = process.env["API_URL"];
  if (typeof apiUrl === "undefined" || apiUrl.trim() === "") {
    throw new Error("API_URL must be configured before using the API client.");
  }

  apiClient ??= createClient<paths>({ baseUrl: apiUrl });
  return apiClient;
};

export default createApiClient;
