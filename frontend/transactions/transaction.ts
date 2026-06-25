import {
  TransactionAccountTypeModel,
  TransactionSortOrderModel,
  TransactionTrendsModeModel,
  TransactionTypeModel,
  type components,
} from "@/framework/data/api";

type Transaction = components["schemas"]["TransactionModel"];
type TransactionTrends = components["schemas"]["TransactionTrendsModel"];
type CurrentTransactions = components["schemas"]["CurrentTransactionsModel"];
type TransactionTrendsTransactionTypeSummary =
  components["schemas"]["TransactionTrendsTransactionTypeSummaryModel"];
type TransactionTrendsDateSummary =
  components["schemas"]["TransactionTrendsDateSummaryModel"];
type TransactionTrendsPeriodSummary =
  components["schemas"]["TransactionTrendsPeriodSummaryModel"];
type TransactionAccount = components["schemas"]["TransactionAccountModel"];
type TransactionFund = components["schemas"]["TransactionFundModel"];
type CreateTransactionRequest = components["schemas"]["CreateTransactionModel"];
type UpdateTransactionRequest = components["schemas"]["UpdateTransactionModel"];
type PostTransactionRequest = components["schemas"]["PostTransactionModel"];

interface TransactionPostingAccount {
  readonly accountId: string;
  readonly accountName: string;
  readonly postedDate: string | null;
}

const summarizeValues = function (values: string[]): string {
  const meaningfulValues = values.filter((value) => value.trim() !== "");
  if (meaningfulValues.length === 0) {
    return "";
  }
  if (meaningfulValues.length === 1) {
    return meaningfulValues[0] ?? "";
  }
  return `${meaningfulValues[0] ?? ""} +${meaningfulValues.length - 1} more`;
};

/**
 * Gets the source label for the provided transaction.
 */
const getTransactionSourceLabel = function (transaction: Transaction): string {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return spendingTransaction.source.account.accountName;
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return (
      incomeTransaction.source.account?.accountName ??
      incomeTransaction.source.location ??
      ""
    );
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return (
      accountTransaction.source.account?.accountName ??
      accountTransaction.source.location ??
      ""
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return fundTransaction.source.fund.fundName;
  }

  return "";
};

/**
 * Gets the destination label for the provided transaction.
 */
const getTransactionDestinationLabel = function (
  transaction: Transaction,
): string {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return summarizeValues(
      spendingTransaction.destinations.map(
        (destination) =>
          destination.account?.accountName ?? destination.location ?? "",
      ),
    );
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return summarizeValues(
      incomeTransaction.destinations.map(
        (destination) => destination.account.accountName,
      ),
    );
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    return summarizeValues(
      accountTransaction.destinations.map(
        (destination) =>
          destination.account?.accountName ?? destination.location ?? "",
      ),
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return summarizeValues(
      fundTransaction.destinations.map(
        (destination) => destination.fund.fundName,
      ),
    );
  }

  return "";
};

/**
 * Gets the account IDs involved in the provided transaction.
 */
const getTransactionAccountIds = function (transaction: Transaction): string[] {
  const accountIds = new Set<string>();

  const addAccount = function (
    account: TransactionAccount | null | undefined,
  ): void {
    if (account !== null && typeof account !== "undefined") {
      accountIds.add(account.accountId);
    }
  };

  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    addAccount(spendingTransaction.source.account);
    spendingTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
    return Array.from(accountIds);
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    addAccount(incomeTransaction.source.account);
    incomeTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
    return Array.from(accountIds);
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    addAccount(accountTransaction.source.account);
    accountTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
  }

  return Array.from(accountIds);
};

/**
 * Gets the fund IDs involved in the provided transaction.
 */
const getTransactionFundIds = function (transaction: Transaction): string[] {
  const fundIds = new Set<string>();

  const addFund = function (fund: TransactionFund | null | undefined): void {
    if (fund !== null && typeof fund !== "undefined") {
      fundIds.add(fund.fundId);
    }
  };

  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    spendingTransaction.destinations.forEach((destination) => {
      destination.fundAssignments.forEach((fundAssignment) => {
        addFund(fundAssignment);
      });
    });
    return Array.from(fundIds);
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    incomeTransaction.destinations.forEach((destination) => {
      destination.fundAssignments.forEach((fundAssignment) => {
        addFund(fundAssignment);
      });
    });
    return Array.from(fundIds);
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    addFund(fundTransaction.source.fund);
    fundTransaction.destinations.forEach((destination) => {
      addFund(destination.fund);
    });
  }

  return Array.from(fundIds);
};

const collectTransactionPostingAccounts = function (
  transaction: Transaction,
): TransactionPostingAccount[] {
  const accounts = new Map<string, TransactionPostingAccount>();

  const addAccount = function (
    account: TransactionAccount | null | undefined,
  ): void {
    if (account === null || typeof account === "undefined") {
      return;
    }
    accounts.set(account.accountId, {
      accountId: account.accountId,
      accountName: account.accountName,
      postedDate: account.postedDate,
    });
  };

  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    addAccount(spendingTransaction.source.account);
    spendingTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
    return Array.from(accounts.values());
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    addAccount(incomeTransaction.source.account);
    incomeTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
    return Array.from(accounts.values());
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    addAccount(accountTransaction.source.account);
    accountTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
  }

  return Array.from(accounts.values());
};

/**
 * Gets the accounts involved in the provided transaction that have not been posted.
 */
const getPostableTransactionAccounts = function (
  transaction: Transaction,
): { accountId: string; accountName: string }[] {
  return collectTransactionPostingAccounts(transaction)
    .filter((account) => account.postedDate === null)
    .map((account) => ({
      accountId: account.accountId,
      accountName: account.accountName,
    }));
};

/**
 * Gets the accounts involved in the provided transaction that have been posted.
 */
const getPostedTransactionAccounts = function (
  transaction: Transaction,
): { accountId: string; accountName: string; postedDate: string | null }[] {
  return collectTransactionPostingAccounts(transaction)
    .filter((account) => account.postedDate !== null)
    .map((account) => ({
      accountId: account.accountId,
      accountName: account.accountName,
      postedDate: account.postedDate,
    }));
};

export {
  type Transaction,
  type TransactionTrends,
  type CurrentTransactions,
  type TransactionTrendsDateSummary,
  type TransactionTrendsPeriodSummary,
  type TransactionTrendsTransactionTypeSummary,
  type TransactionAccount,
  type TransactionFund,
  type CreateTransactionRequest,
  type UpdateTransactionRequest,
  type PostTransactionRequest,
  TransactionTrendsModeModel as TransactionTrendsMode,
  TransactionSortOrderModel as TransactionSortOrder,
  TransactionAccountTypeModel as TransactionAccountType,
  TransactionTypeModel as TransactionType,
  getTransactionSourceLabel,
  getTransactionDestinationLabel,
  getTransactionAccountIds,
  getTransactionFundIds,
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
};
