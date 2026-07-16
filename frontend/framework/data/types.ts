import { BalanceEventTypeModel, type components } from "@/framework/data/api";

/**
 * Type representing an Income Amount, including total, tracked, and untracked amounts.
 */
type IncomeAmount = components["schemas"]["IncomeAmountModel"];

export { type IncomeAmount, BalanceEventTypeModel as BalanceEventType };
