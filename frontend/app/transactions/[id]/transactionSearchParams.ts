/**
 * Type representing the search params for the Transaction page.
 */
interface TransactionSearchParams {
  accountId: string;
  accountingPeriodId?: string;
  fundId?: string;
}

export default TransactionSearchParams;
