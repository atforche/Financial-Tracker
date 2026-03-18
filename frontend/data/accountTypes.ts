import {
  AccountTypeModel,
  AccountingPeriodAccountSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

/**
 * Type representing a request to create an account.
 */
type CreateAccountRequest = components["schemas"]["CreateAccountModel"];

export {
  type Account,
  type CreateAccountRequest,
  AccountingPeriodAccountSortOrderModel as AccountingPeriodAccountSortOrder,
  AccountTypeModel as AccountType,
};
