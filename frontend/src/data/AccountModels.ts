import type AccountType from "@core/fieldValues/AccountType";

/**
 * Type representing the key value of an account.
 */
type AccountId = bigint;

/**
 * Interface representing an Account.
 * @param {AccountId} id - Id for this Account.
 * @param {string} name - Name for this Account.
 * @param {AccountType} type - Type for this Account.
 * @param {boolean} isActive - Is Active flag for this Account.
 */
interface Account {
  id: AccountId;
  name: string;
  type: AccountType;
  isActive: boolean;
}

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
 * Interface representing a request to update an Account.
 * @param {string | null} name - New name for the Account being updated.
 * @param {boolean | null} isActive - New is active flag for the Account being updated.
 */
interface UpdateAccountRequest {
  name?: string | null;
  isActive?: boolean | null;
}

export type { AccountId, Account, CreateAccountRequest, UpdateAccountRequest };
