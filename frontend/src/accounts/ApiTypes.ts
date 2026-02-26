import {
  AccountSortOrderModel,
  AccountTransactionSortOrderModel,
  AccountTypeModel,
  type components,
} from "@data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

/**
 * Interface representing an identifier for an Account.
 */
interface AccountIdentifier {
  id: string;
  name: string;
}

/**
 * Type representing a request to create an Account.
 */
type CreateAccountRequest = components["schemas"]["CreateAccountModel"];

/**
 * Type representing a request to update an Account.
 */
type UpdateAccountRequest = components["schemas"]["UpdateAccountModel"];

export {
  type Account,
  AccountTypeModel as AccountType,
  type CreateAccountRequest,
  type AccountIdentifier,
  AccountSortOrderModel as AccountSortOrder,
  AccountTransactionSortOrderModel as AccountTransactionSortOrder,
  type UpdateAccountRequest,
};
