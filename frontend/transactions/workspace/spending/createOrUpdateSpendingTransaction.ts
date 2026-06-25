import {
  CreateTransactionModelCreateSpendingTransactionModelType,
  UpdateTransactionModelUpdateSpendingTransactionModelType,
} from "@/framework/data/api";
import { type FundAmount, hasIncompleteFundAssignments } from "@/funds/types";

/**
 * Determines if the provided spending transaction is complete.
 */
const isSpendingTransactionComplete = function (
  spendingFundAssignments: FundAmount[],
): boolean {
  return (
    !hasIncompleteFundAssignments(spendingFundAssignments) &&
    spendingFundAssignments.every(
      (fundAmount) =>
        fundAmount.fundName !== "Unassigned" || fundAmount.amount === 0,
    )
  );
};

export {
  CreateTransactionModelCreateSpendingTransactionModelType as CreateSpendingTransactionType,
  UpdateTransactionModelUpdateSpendingTransactionModelType as UpdateSpendingTransactionType,
  isSpendingTransactionComplete,
};
