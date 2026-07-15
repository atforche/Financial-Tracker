import type {
  Transaction,
  TransactionAccount,
  TransactionFund,
  TransactionGoal,
} from "@/transactions/transaction";
import { asAccountTransaction } from "@/transactions/accountTransaction";
import { asFundTransaction } from "@/transactions/fundTransaction";
import { asIncomeTransaction } from "@/transactions/incomeTransaction";
import { asSpendingTransaction } from "@/transactions/spendingTransaction";

interface TransactionPostingAccount {
  readonly accountId: string;
  readonly accountName: string;
  readonly postedDate: string | null;
}

/**
 * Gets the account IDs involved in the provided transaction.
 */
const getTransactionAccountIds = function (transaction: Transaction): string[] {
  const accountIds = new Set<string>();

  const addAccount = function (
    account: TransactionAccount | null | undefined,
  ): void {
    if (account !== null && typeof account !== "undefined") {
      accountIds.add(account.account.id);
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
 * Gets the Account balance events captured by the provided Transaction.
 */
const getTransactionAccountBalanceEvents = function (
  transaction: Transaction,
): TransactionAccount[] {
  const events: TransactionAccount[] = [];
  const addAccount = function (
    account: TransactionAccount | null | undefined,
  ): void {
    if (account !== null && typeof account !== "undefined") {
      events.push(account);
    }
  };

  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    addAccount(spendingTransaction.source.account);
    spendingTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
    return events;
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    addAccount(incomeTransaction.source.account);
    incomeTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
    return events;
  }

  const accountTransaction = asAccountTransaction(transaction);
  if (accountTransaction !== null) {
    addAccount(accountTransaction.source.account);
    accountTransaction.destinations.forEach((destination) => {
      addAccount(destination.account);
    });
  }
  return events;
};

/**
 * Gets the fund IDs involved in the provided transaction.
 */
const getTransactionFundIds = function (transaction: Transaction): string[] {
  const fundIds = new Set<string>();

  const addFund = function (fund: TransactionFund | null | undefined): void {
    if (fund !== null && typeof fund !== "undefined") {
      fundIds.add(fund.fund.id);
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

/**
 * Gets the Fund balance events captured by the provided Transaction.
 */
const getTransactionFundBalanceEvents = function (
  transaction: Transaction,
): TransactionFund[] {
  const events: TransactionFund[] = [];
  const addFund = function (fund: TransactionFund): void {
    events.push(fund);
  };

  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    spendingTransaction.destinations.forEach((destination) => {
      destination.fundAssignments.forEach(addFund);
    });
    return events;
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    incomeTransaction.destinations.forEach((destination) => {
      destination.fundAssignments.forEach(addFund);
    });
    return events;
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    addFund(fundTransaction.source.fund);
    fundTransaction.destinations.forEach((destination) => {
      addFund(destination.fund);
    });
  }
  return events;
};

/**
 * Gets the Goal balance events captured by the provided Transaction.
 */
const getTransactionGoalBalanceEvents = function (
  transaction: Transaction,
): TransactionGoal[] {
  const spendingTransaction = asSpendingTransaction(transaction);
  if (spendingTransaction !== null) {
    return spendingTransaction.destinations.flatMap(
      (destination) => destination.goals,
    );
  }

  const incomeTransaction = asIncomeTransaction(transaction);
  if (incomeTransaction !== null) {
    return incomeTransaction.destinations.flatMap(
      (destination) => destination.goals,
    );
  }

  const fundTransaction = asFundTransaction(transaction);
  if (fundTransaction !== null) {
    return [
      fundTransaction.source.goal,
      ...fundTransaction.destinations.map((destination) => destination.goal),
    ].filter((goal): goal is TransactionGoal => goal !== null);
  }
  return [];
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
    accounts.set(account.account.id, {
      accountId: account.account.id,
      accountName: account.account.name,
      postedDate: account.date ?? null,
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
  getTransactionAccountIds,
  getTransactionAccountBalanceEvents,
  getTransactionFundIds,
  getTransactionFundBalanceEvents,
  getTransactionGoalBalanceEvents,
  getPostableTransactionAccounts,
  getPostedTransactionAccounts,
};
