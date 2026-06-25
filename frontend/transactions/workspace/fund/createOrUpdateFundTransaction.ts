import {
  CreateTransactionModelCreateFundTransactionModelType,
  UpdateTransactionModelUpdateFundTransactionModelType,
} from "@/framework/data/api";
import type { Fund } from "@/funds/types";

/**
 * Determines if the provided fund transaction is complete.
 */
const isFundTransactionComplete = function (
  debitFund: Fund | null,
  creditFund: Fund | null,
): boolean {
  return debitFund !== null && creditFund !== null;
};

export {
  CreateTransactionModelCreateFundTransactionModelType as CreateFundTransactionType,
  UpdateTransactionModelUpdateFundTransactionModelType as UpdateFundTransactionType,
  isFundTransactionComplete,
};
