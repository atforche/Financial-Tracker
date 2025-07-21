import { Fund, type FundKey } from "@data/Fund";

const funds = [
  new Fund({
    name: "Spend",
    description: "Fund containing money that's spent every month",
  }),
  new Fund({
    name: "Reserve",
    description:
      "Fund containing money that's spent regularly but not necessary every month",
  }),
  new Fund({
    name: "Saving",
    description: "Fund containing money that's being saved",
  }),
];

/**
 * Interface representing a request to create an Fund.
 * @param {string} name - Name for this Fund.
 * @param {string} description - Description for this Fund.
 */
interface CreateFundRequest {
  name: string;
  description: string | null;
}

/**
 * Creates a new Fund and returns it.
 * @param {CreateFundRequest} request - Request to create a new Fund.
 * @returns {Fund} The newly created Fund.
 */
const addFund = function (request: CreateFundRequest): Fund {
  const fund = new Fund(request);
  funds.push(fund);
  return fund;
};

/**
 * Retrieves all Funds.
 * @returns {Fund[]} An array of all Funds.
 */
const getAllFunds = function (): Fund[] {
  return funds;
};

/**
 * Retrieves the Fund that matches the following key, or null if one is not found.
 * @param {FundKey} key - Key of the Fund to retrieve.
 * @returns {Fund | null} Fund that matches the provided key, or null.
 */
const getFundByKey = function (key: FundKey): Fund | null {
  const foundFund = funds.find((fund) => fund.key === key);
  if (!foundFund) {
    return null;
  }
  return foundFund;
};

/**
 * Interface representing a request to update an Fund.
 * @param {string | null} name - New name for the Fund being updated.
 * @param {string | null} description - New description for the Fund being updated.
 */
interface UpdateFundRequest {
  name?: string | null;
  description?: string | null;
}

/**
 * Updates the Fund identified by the provided key.
 * @param {FundKey} key - Key of the Fund to update.
 * @param {UpdateFundRequest} request - Request to update a Fund.
 * @returns {Fund} The updated Fund.
 * @throws An error if no Funds match the provided key.
 */
const updateFund = function (
  key: FundKey,
  { name = null, description = null }: UpdateFundRequest,
): Fund {
  const elementsToRemove = 1;
  const fund = getFundByKey(key);
  if (fund === null) {
    throw new Error("Invalid key for fund");
  }
  if (name !== null) {
    fund.name = name;
  }
  if (description !== null) {
    fund.description = description;
  }
  const index = funds.findIndex((a) => a.key === key);
  funds.splice(index, elementsToRemove);
  funds.push(fund);
  return fund;
};

/**
 * Deletes the Fund identified by the provided key.
 * @param {FundKey} key - Key of the Fund to delete.
 * @throws An error if no Funds match the provided key.
 */
const deleteFund = function (key: FundKey): void {
  const elementsToRemove = 1;
  const fund = getFundByKey(key);
  if (fund === null) {
    throw new Error("Invalid key for fund");
  }
  const index = funds.findIndex((f) => f.key === key);
  funds.splice(index, elementsToRemove);
};

export {
  type CreateFundRequest,
  addFund,
  getAllFunds,
  getFundByKey,
  type UpdateFundRequest,
  updateFund,
  deleteFund,
};
