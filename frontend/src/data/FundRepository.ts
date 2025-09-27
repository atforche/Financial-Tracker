import type { components } from "@data/api";

type Fund = components["schemas"]["FundModel"];
type CreateOrUpdateFundRequest =
  components["schemas"]["CreateOrUpdateFundModel"];

/**
 * Creates a new Fund and returns it.
 * @param {CreateOrUpdateFundRequest} request - Request to create a new Fund.
 * @returns {Fund} The newly created Fund.
 */
const addFund = async function (
  request: CreateOrUpdateFundRequest,
): Promise<Fund> {
  const response = await fetch("http://localhost:8080/funds", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });
  return (await response.json()) as Fund;
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
  const response = await fetch(`http://localhost:8080/funds/${fund.id}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });
  return (await response.json()) as Fund;
};

/**
 * Retrieves all Funds.
 * @returns {Fund[]} An array of all Funds.
 */
const getAllFunds = async function (): Promise<Fund[]> {
  const response = await fetch("http://localhost:8080/funds");
  return (await response.json()) as Fund[];
};

/**
 * Retrieves the Fund that matches the provided ID, or null if one is not found.
 * @param {string} id - ID of the Fund to retrieve.
 * @returns {Fund | null} Fund that matches the provided ID, or null.
 */
const getFundById = async function (id: string): Promise<Fund | null> {
  const response = await fetch(`http://localhost:8080/funds/${id}`);
  if (!response.ok) {
    return null;
  }
  return (await response.json()) as Fund;
};

export {
  type Fund,
  type CreateOrUpdateFundRequest,
  addFund,
  updateFund,
  getAllFunds,
  getFundById,
};
