import type { components } from "@data/api";

/**
 * Type representing an Accounting Period.
 */
type AccountingPeriod = components["schemas"]["AccountingPeriodModel"];

/**
 * Type representing a request to create an Accounting Period.
 */
type CreateAccountingPeriodRequest =
  components["schemas"]["CreateAccountingPeriodModel"];

export { type AccountingPeriod, type CreateAccountingPeriodRequest };
