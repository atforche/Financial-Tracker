import { AccountingPeriodSortOrderModel, type components } from "@/data/api";
import dayjs, { type Dayjs } from "dayjs";

/**
 * Type representing an Accounting Period.
 */
type AccountingPeriod = components["schemas"]["AccountingPeriodModel"];

/**
 * Interface representing an identifier for an Accounting Period.
 */
interface AccountingPeriodIdentifier {
  id: string;
  name: string;
}

/**
 * Type representing a request to create an Accounting Period.
 */
type CreateAccountingPeriodRequest =
  components["schemas"]["CreateAccountingPeriodModel"];

/**
 * Gets the minimum date associated with the provided accounting period.
 */
const getMinimumDate = function (accountingPeriod: AccountingPeriod): Dayjs {
  return dayjs(accountingPeriod.name, "MMMM YYYY").subtract(1, "month");
};

/**
 * Gets the maximum date associated with the provided accounting period.
 */
const getMaximumDate = function (accountingPeriod: AccountingPeriod): Dayjs {
  return dayjs(accountingPeriod.name, "MMMM YYYY")
    .add(2, "month")
    .subtract(1, "day");
};

/**
 * Gets the default date associated with the provided accounting period.
 */
const getDefaultDate = function (
  accountingPeriod: AccountingPeriod | null,
): Dayjs | null {
  if (accountingPeriod === null) {
    return null;
  }
  return dayjs(accountingPeriod.name, "MMMM YYYY");
};

export {
  type AccountingPeriod,
  type CreateAccountingPeriodRequest,
  type AccountingPeriodIdentifier,
  getMinimumDate,
  getMaximumDate,
  getDefaultDate,
  AccountingPeriodSortOrderModel as AccountingPeriodSortOrder,
};
