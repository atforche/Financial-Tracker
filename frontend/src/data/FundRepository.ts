import type { components } from "@data/api";

type Fund = components["schemas"]["FundModel"];
type CreateFundRequest = components["schemas"]["CreateFundModel"];

/**
 * Creates a new Fund and returns it.
 * @param {CreateFundRequest} request - Request to create a new Fund.
 * @returns {Fund} The newly created Fund.
 */
const addFund = async function (request: CreateFundRequest): Promise<Fund> {
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

export { type Fund, type CreateFundRequest, addFund, getAllFunds, getFundById };
