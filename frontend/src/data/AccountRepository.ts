import type {
  Account,
  AccountId,
  CreateAccountRequest,
  UpdateAccountRequest,
} from "@data/AccountModels";

/**
 * Creates a new Account and returns it.
 * @param {CreateAccountRequest} request - Request to create a new Account.
 * @returns {Promise<Account>} The newly created Account.
 */
const addAccount = async function (
  request: CreateAccountRequest,
): Promise<Account> {
  const response = await fetch("http://localhost:8080/accounts/", {
    method: "POST",
    body: JSON.stringify(request),
  });
  const data = (await response.json()) as Promise<Account>;
  return data;
};

/**
 * Retrieves all Accounts.
 * @returns {Promise<Account[]>} An array of all Accounts.
 */
const getAllAccounts = async function (): Promise<Account[]> {
  const response = await fetch("http://localhost:8080/accounts");
  const data = (await response.json()) as Promise<Account[]>;
  return data;
};

/**
 * Retrieves the Account that matches the provided id, or null if one is not found.
 * @param {AccountId} id - ID of the Account to retrieve.
 * @returns {Promise<Account | null>} Account that matches the provided key, or null.
 */
const getAccountById = async function (id: AccountId): Promise<Account | null> {
  const response = await fetch(
    `http://localhost:8080/accounts/${id.toString()}`,
  );
  if (!response.ok) {
    return null;
  }
  const data = (await response.json()) as Promise<Account>;
  return data;
};

/**
 * Updates the Account with the provided ID.
 * @param {AccountId} id - ID of the Account to update.
 * @param {UpdateAccountRequest} request - Request to update an Account.
 * @returns {Promise<Account>} The updated Account.
 * @throws An error if no Accounts match the provided key.
 */
const updateAccount = async function (
  id: AccountId,
  request: UpdateAccountRequest,
): Promise<Account> {
  const response = await fetch(
    `http://localhost:8080/accounts/${id.toString()}`,
    {
      method: "POST",
      body: JSON.stringify(request),
    },
  );
  const data = (await response.json()) as Promise<Account>;
  return data;
};

/**
 * Deletes the Account identified by the provided ID.
 * @param {AccountId} id - ID of the Account to delete.
 * @throws An error if no Accounts match the provided key.
 */
const deleteAccount = async function (id: AccountId): Promise<void> {
  await fetch(`http://localhost:8080/accounts/${id.toString()}`, {
    method: "DELETE",
  });
};

export {
  addAccount,
  getAllAccounts,
  getAccountById,
  updateAccount,
  deleteAccount,
};
