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

export {
  type Account,
  type AccountingPeriodAccount,
  type AccountIdentifier,
  type CreateAccountRequest,
  AccountingPeriodAccountSortOrderModel as AccountingPeriodAccountSortOrder,
  AccountTypeModel as AccountType,
};
