import {
  CreateTransactionModelCreateIncomeTransactionModelType,
  UpdateTransactionModelUpdateIncomeTransactionModelType,
} from "@/framework/data/api";
import { type FundAmount, hasIncompleteFundAssignments } from "@/funds/types";

/**
 * Determines if the provided income transaction is complete.
 */
const isIncomeTransactionComplete = function (
  incomeFundAssignments: FundAmount[],
): boolean {
  return !hasIncompleteFundAssignments(incomeFundAssignments);
};

export {
  CreateTransactionModelCreateIncomeTransactionModelType as CreateIncomeTransactionType,
  UpdateTransactionModelUpdateIncomeTransactionModelType as UpdateIncomeTransactionType,
  isIncomeTransactionComplete,
};
