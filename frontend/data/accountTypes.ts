import {
  AccountSortOrderModel,
  AccountTransactionSortOrderModel,
  AccountTypeModel,
  AccountingPeriodAccountSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

/**
 * Type representing an Account in the context of an Accounting Period.
 */
type AccountingPeriodAccount =
  components["schemas"]["AccountingPeriodAccountModel"];

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
  type AccountingPeriodAccount,
  type AccountIdentifier,
  type CreateAccountRequest,
  type UpdateAccountRequest,
  AccountSortOrderModel as AccountSortOrder,
  AccountingPeriodAccountSortOrderModel as AccountingPeriodAccountSortOrder,
  AccountTransactionSortOrderModel as AccountTransactionSortOrder,
  AccountTypeModel as AccountType,
  isPositiveChangeInBalance,
};
