import type { components, paths } from "@data/api";
import createClient from "openapi-fetch";

type Fund = components["schemas"]["FundModel"];
type CreateOrUpdateFundRequest =
  components["schemas"]["CreateOrUpdateFundModel"];

const client = createClient<paths>({
  baseUrl: "http://localhost:8080",
});

/**
 * Creates a new Fund and returns it.
 * @param {CreateOrUpdateFundRequest} request - Request to create a new Fund.
 * @returns {Fund} The newly created Fund.
 */
const addFund = async function (
  request: CreateOrUpdateFundRequest,
): Promise<Fund> {
  const { data, error } = await client.POST("/funds", {
    body: request,
  });
  if (typeof error !== "undefined") {
    throw new Error(`Failed to add fund: ${error.message}`);
  }
  return data;
};

/**
 * Updates an existing Fund.
 * @param {Fund} fund - Existing Fund to update.
 * @param {CreateOrUpdateFundRequest} request - Request to update the Fund.
 * @returns {Fund} The updated Fund.
 */
const updateFund = async function (
  fund: Fund,
  request: CreateOrUpdateFundRequest,
): Promise<Fund> {
  const { data, error } = await client.POST("/funds/{fundId}", {
    params: {
      path: {
        fundId: fund.id,
      },
    },
    body: request,
  });
  if (typeof error !== "undefined") {
    throw new Error(`Failed to update fund: ${error.message}`);
  }
  return data;
};

/**
 * Deletes an existing Fund.
 * @param {Fund} fund - Fund to delete.
 */
const deleteFund = async function (fund: Fund): Promise<void> {
  const { error } = await client.DELETE("/funds/{fundId}", {
    params: {
      path: {
        fundId: fund.id,
      },
    },
  });
  if (typeof error !== "undefined") {
    throw new Error(`Failed to delete fund: ${error.message}`);
  }
};

/**
 * Retrieves all Funds.
 * @returns {Fund[]} An array of all Funds.
 */
const getAllFunds = async function (): Promise<Fund[]> {
  const { data, error } = await client.GET("/funds");
  if (typeof error !== "undefined") {
    throw new Error(`Failed to get funds`);
  }
  return data;
};

export {
  type Fund,
  type CreateOrUpdateFundRequest,
  addFund,
  updateFund,
  deleteFund,
  getAllFunds,
};
