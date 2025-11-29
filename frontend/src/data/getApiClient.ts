import createClient, { type Client } from "openapi-fetch";
import type { paths } from "@data/api";

/**
 * Gets the API client for making requests to the backend.
 * @returns The API client to use for making requests.
 */
const getApiClient = function (): Client<paths> {
  return createClient<paths>({
    baseUrl: import.meta.env.VITE_API_URL,
  });
};

export default getApiClient;
