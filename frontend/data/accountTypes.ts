import {
  AccountingPeriodAccountSortOrderModel,
  type components,
} from "@/data/api";

/**
 * Type representing an Account.
 */
type Account = components["schemas"]["AccountModel"];

export {
  type Account,
  AccountingPeriodAccountSortOrderModel as AccountingPeriodAccountSortOrder,
};
