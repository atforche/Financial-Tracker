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

export {
  type Account,
  type AccountIdentifier,
  type CreateAccountRequest,
  AccountingPeriodAccountSortOrderModel as AccountingPeriodAccountSortOrder,
  AccountTypeModel as AccountType,
};
