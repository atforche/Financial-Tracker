import {
  AccountSortOrderModel,
  AccountTransactionSortOrderModel,
  AccountTypeModel,
  type components,
} from "@/framework/data/api";

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
 * Type representing a request to create an account.
 */
type CreateAccountRequest = components["schemas"]["CreateAccountModel"];

/**
 * Type representing a request to update an account.
 */
type UpdateAccountRequest = components["schemas"]["UpdateAccountModel"];

/**
 * Determines if the provided account type supports tracked fund assignments.
 */
const isTrackedAccountType = function (accountType: AccountTypeModel): boolean {
  switch (accountType) {
    case AccountTypeModel.Standard:
    case AccountTypeModel.CreditCard:
    case AccountTypeModel.Investment:
      return true;
    case AccountTypeModel.Debt:
    case AccountTypeModel.Retirement:
    case AccountTypeModel.Escrow:
      return false;
    default:
      return false;
  }
};

/**
 * Determines if the provided change in balance is "positive" based on the provided account type.
 */
const isPositiveChangeInBalance = function (
  accountType: AccountTypeModel,
  changeInBalance: number,
): boolean {
  if (accountType === AccountTypeModel.Debt) {
    return changeInBalance <= 0;
  }
  return changeInBalance >= 0;
};

export {
  type Account,
  type AccountIdentifier,
  type CreateAccountRequest,
  type UpdateAccountRequest,
  AccountSortOrderModel as AccountSortOrder,
  AccountTransactionSortOrderModel as AccountTransactionSortOrder,
  AccountTypeModel as AccountType,
  isTrackedAccountType,
  isPositiveChangeInBalance,
};
