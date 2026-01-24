import { AccountTypeModel, type components } from "@data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

/**
 * Type representing a request to create an Account.
 */
type CreateAccountRequest = components["schemas"]["CreateAccountModel"];

export {
  type Account,
  AccountTypeModel as AccountType,
  type CreateAccountRequest,
};
