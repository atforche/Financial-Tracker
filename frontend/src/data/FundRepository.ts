import type { components, paths } from "@data/api";
import type { ApiError } from "@data/ApiError";
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
 * @returns {Fund | ApiError} The newly created Fund, or an ApiError if the operation failed.
 */
const addFund = async function (
  request: CreateOrUpdateFundRequest,
): Promise<Fund | ApiError> {
  const { data, error } = await client.POST("/funds", {
    body: request,
  });
  if (typeof error !== "undefined") {
    return error;
  }
  return data;
};

/**
 * Updates an existing Fund.
 * @param {Fund} fund - Existing Fund to update.
 * @param {CreateOrUpdateFundRequest} request - Request to update the Fund.
 * @returns {Fund | ApiError} The updated Fund, or an ApiError if the operation failed.
 */
const updateFund = async function (
  fund: Fund,
  request: CreateOrUpdateFundRequest,
): Promise<Fund | ApiError> {
  const { data, error } = await client.POST("/funds/{fundId}", {
    params: {
      path: {
        fundId: fund.id,
      },
    },
    body: request,
  });
  if (typeof error !== "undefined") {
    return error;
  }
  return data;
};

/**
 * Deletes an existing Fund.
 * @param {Fund} fund - Fund to delete.
 * @returns {ApiError | null} An ApiError if the operation failed, or null if it succeeded.
 */
const deleteFund = async function (fund: Fund): Promise<ApiError | null> {
  const { error } = await client.DELETE("/funds/{fundId}", {
    params: {
      path: {
        fundId: fund.id,
      },
    },
  });
  if (typeof error !== "undefined") {
    return error;
  }
  return null;
};

/**
 * Retrieves all Funds.
 * @returns {Fund[] | ApiError} An array of all Funds, or an ApiError if the operation failed.
 */
const getAllFunds = async function (): Promise<Fund[] | ApiError> {
  const { data, error } = await client.GET("/funds");
  if (typeof error !== "undefined") {
    return error;
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
