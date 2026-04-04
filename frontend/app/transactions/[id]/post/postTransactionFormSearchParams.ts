/**
 * Type representing the search params for the Post Transaction Form.
 */
interface PostTransactionFormSearchParams {
  account: "debit" | "credit";
  accountId: string;
  accountingPeriodId?: string;
  fundId?: string;
}

export type { PostTransactionFormSearchParams };
