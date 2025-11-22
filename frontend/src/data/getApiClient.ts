import createClient, { type Client } from "openapi-fetch";
import type { paths } from "@data/api";

/**
 * Gets the API client for making requests to the backend.
 * @returns {Client<paths>} The API client to use for making requests.
 */
const getApiClient = function (): Client<paths> {
  return createClient<paths>({
    baseUrl: "http://localhost:8080",
  });
};

export default getApiClient;
