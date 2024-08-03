import { Account, type AccountKey, AccountType } from "@data/Account";

const accounts = [
  new Account({ name: "Checking", type: AccountType.Standard }),
  new Account({ name: "Savings", type: AccountType.Standard }),
  new Account({ name: "Retirement", type: AccountType.Investment }),
  new Account({ name: "Loan", type: AccountType.Debt }),
  new Account({
    name: "Old",
    type: AccountType.Standard,
    isActive: false,
  }),
];

/**
 * Interface representing a request to create an Account.
 * @param {string} name - Name for this Account.
 * @param {AccountType} type - Type for this Account.
 * @param {boolean} isActive - Boolean flag indicating whether this Account is active. Defaults to true if not provided.
 */
interface CreateAccountRequest {
  name: string;
  type: AccountType;
  isActive?: boolean;
}

/**
 * Creates a new Account and returns it.
 * @param {CreateAccountRequest} request - Request to create a new Account.
 * @returns {Account} The newly created Account.
 */
const addAccount = function (request: CreateAccountRequest): Account {
  const account = new Account(request);
  accounts.push(account);
  return account;
};

/**
 * Retrieves all Accounts.
 * @returns {Account[]} An array of all Accounts.
 */
const getAllAccounts = function (): Account[] {
  return accounts;
};

/**
 * Retrieves the Account that matches the following key, or null if one is not found.
 * @param {AccountKey} key - Key of the Account to retrieve.
 * @returns {Account | null} Account that matches the provided key, or null.
 */
const getAccountByKey = function (key: AccountKey): Account | null {
  const foundAccount = accounts.find((account) => account.key === key);
  if (!foundAccount) {
    return null;
  }
  return foundAccount;
};

/**
 * Interface representing a request to update an Account.
 * @param {string | null} name - New name for the Account being updated.
 * @param {boolean | null} isActive - New is active flag for the Account being updated.
 */
interface UpdateAccountRequest {
  isActive?: boolean | null;
  name?: string | null;
}

/**
 * Updates the Account identified by the provided key.
 * @param {AccountKey} key - Key of the Account to update.
 * @param {UpdateAccountRequest} request - Request to update an Account.
 * @returns {Account} The updated Account.
 * @throws An error if no Accounts match the provided key.
 */
const updateAccount = function (
  key: AccountKey,
  { name = null, isActive = null }: UpdateAccountRequest,
): Account {
  const elementsToRemove = 1;
  const account = getAccountByKey(key);
  if (account === null) {
    throw new Error("Invalid key for account");
  }
  if (name !== null) {
    account.name = name;
  }
  if (isActive !== null) {
    account.isActive = isActive;
  }
  const index = accounts.findIndex((a) => a.key === key);
  accounts.splice(index, elementsToRemove);
  accounts.push(account);
  return account;
};

/**
 * Deletes the Account identified by the provided key.
 * @param {AccountKey} key - Key of the Account to delete.
 * @throws An error if no Accounts match the provided key.
 */
const deleteAccount = function (key: AccountKey): void {
  const elementsToRemove = 1;
  const account = getAccountByKey(key);
  if (account === null) {
    throw new Error("Invalid key for account");
  }
  const index = accounts.findIndex((a) => a.key === key);
  accounts.splice(index, elementsToRemove);
};

export {
  type CreateAccountRequest,
  addAccount,
  getAllAccounts,
  getAccountByKey,
  type UpdateAccountRequest,
  updateAccount,
  deleteAccount,
};
