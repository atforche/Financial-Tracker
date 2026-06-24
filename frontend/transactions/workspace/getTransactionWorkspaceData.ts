import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { Transaction } from "@/transactions/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import getApiClient from "@/framework/data/getApiClient";
import { rowsPerPage } from "@/framework/listframe/Constants";

interface TransactionWorkspaceReferenceData {
  readonly openAccountingPeriods: AccountingPeriod[];
  readonly allAccountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
}

interface TransactionWorkspaceListData extends TransactionWorkspaceReferenceData {
  readonly currentPage: number;
  readonly normalizedAccountingPeriodIds: string[] | null;
  readonly normalizedAccountIds: string[] | null;
  readonly normalizedFundIds: string[] | null;
  readonly transactions: {
    readonly items: Transaction[];
    readonly totalCount: number;
  };
  readonly selectedTransaction: Transaction | null;
  readonly displayedTransactions: Transaction[];
}

const toRepeatedSearchParam = function (
  value: string | string[] | undefined,
): string[] | null {
  if (Array.isArray(value)) {
    return value;
  }

  return typeof value === "string" ? [value] : null;
};

/**
 * Fetches reference data required for the transaction workspace, including accounting periods, accounts, funds, and goals.
 */
const getTransactionWorkspaceReferenceData =
  async function (): Promise<TransactionWorkspaceReferenceData> {
    const apiClient = getApiClient();

    const openAccountingPeriodsPromise = apiClient.GET(
      "/accounting-periods/open",
    );
    const allAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
      params: {
        query: {
          Limit: 500,
        },
      },
    });
    const accountsPromise = apiClient.GET("/accounts");
    const fundsPromise = apiClient.GET("/funds");

    const [
      { data: openAccountingPeriods },
      { data: allAccountingPeriods },
      { data: accounts },
      { data: funds },
    ] = await Promise.all([
      openAccountingPeriodsPromise,
      allAccountingPeriodsPromise,
      accountsPromise,
      fundsPromise,
    ]);

    if (typeof openAccountingPeriods === "undefined") {
      throw new Error("Failed to fetch open accounting periods");
    }
    if (typeof allAccountingPeriods === "undefined") {
      throw new Error("Failed to fetch accounting periods");
    }
    if (typeof accounts === "undefined") {
      throw new Error("Failed to fetch accounts");
    }
    if (typeof funds === "undefined") {
      throw new Error("Failed to fetch funds");
    }

    let assignmentGoals: AssignmentGoal[] = [];
    let spendingGoals: SpendingGoal[] = [];

    if (openAccountingPeriods.length > 0) {
      const [assignmentGoalResponse, spendingGoalResponse] = await Promise.all([
        apiClient.GET("/goals/assignment/many", {
          params: {
            query: {
              AccountingPeriodIds: openAccountingPeriods.map(
                (period) => period.id,
              ),
            },
          },
        }),
        apiClient.GET("/goals/spending/many", {
          params: {
            query: {
              AccountingPeriodIds: openAccountingPeriods.map(
                (period) => period.id,
              ),
            },
          },
        }),
      ]);

      if (typeof assignmentGoalResponse.data === "undefined") {
        throw new Error("Failed to fetch assignment goals");
      }
      if (typeof spendingGoalResponse.data === "undefined") {
        throw new Error("Failed to fetch spending goals");
      }

      assignmentGoals = assignmentGoalResponse.data.items;
      spendingGoals = spendingGoalResponse.data.items;
    }

    return {
      openAccountingPeriods,
      allAccountingPeriods: allAccountingPeriods.items,
      accounts: accounts.items,
      funds: funds.items,
      assignmentGoals,
      spendingGoals,
    };
  };

/**
 * Fetches a transaction by its ID.
 */
const getTransactionById = async function (
  transactionId: string,
): Promise<Transaction | null> {
  const apiClient = getApiClient();
  const { data } = await apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId,
      },
    },
  });

  return data ?? null;
};

/**
 * Fetches the list of transactions within the workspace based on search parameters.
 */
const getTransactionWorkspaceListData = async function (
  searchParams: TransactionWorkspaceSearchParams,
): Promise<TransactionWorkspaceListData> {
  const {
    accountingPeriodIds,
    accountIds,
    fundIds,
    sort,
    page,
    selectedTransactionId,
  } = searchParams;
  const apiClient = getApiClient();
  const currentPage = normalizePageValue(page);
  const normalizedAccountingPeriodIds =
    toRepeatedSearchParam(accountingPeriodIds);
  const normalizedAccountIds = toRepeatedSearchParam(accountIds);
  const normalizedFundIds = toRepeatedSearchParam(fundIds);

  const transactionsPromise = apiClient.GET("/transactions", {
    params: {
      query: {
        ...(normalizedAccountingPeriodIds !== null
          ? { AccountingPeriodIds: normalizedAccountingPeriodIds }
          : {}),
        ...(normalizedAccountIds !== null
          ? { AccountIds: normalizedAccountIds }
          : {}),
        ...(normalizedFundIds !== null ? { FundIds: normalizedFundIds } : {}),
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });
  const selectedTransactionPromise =
    typeof selectedTransactionId === "string"
      ? apiClient.GET("/transactions/{transactionId}", {
          params: {
            path: {
              transactionId: selectedTransactionId,
            },
          },
        })
      : Promise.resolve({ data: null });

  const [
    referenceData,
    { data: transactions },
    { data: selectedTransactionById },
  ] = await Promise.all([
    getTransactionWorkspaceReferenceData(),
    transactionsPromise,
    selectedTransactionPromise,
  ]);

  if (typeof transactions === "undefined") {
    throw new Error("Failed to fetch transactions");
  }

  const selectedTransaction =
    selectedTransactionById ??
    transactions.items.find(
      (transaction) => transaction.id === selectedTransactionId,
    ) ??
    null;

  const displayedTransactions =
    selectedTransaction !== null &&
    !transactions.items.some(
      (transaction) => transaction.id === selectedTransaction.id,
    )
      ? [selectedTransaction, ...transactions.items].slice(0, rowsPerPage)
      : transactions.items;

  return {
    ...referenceData,
    currentPage,
    normalizedAccountingPeriodIds,
    normalizedAccountIds,
    normalizedFundIds,
    transactions,
    selectedTransaction,
    displayedTransactions,
  };
};

export {
  getTransactionById,
  getTransactionWorkspaceListData,
  getTransactionWorkspaceReferenceData,
};
