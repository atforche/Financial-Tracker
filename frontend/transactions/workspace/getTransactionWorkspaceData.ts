import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import {
  getPageOffset,
  normalizePageValue,
  rowsPerPage,
} from "@/framework/listframe/page";
import {
  normalizeStringSearchParams,
  toRepeatedSearchParams,
} from "@/framework/routes/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundWithBalance } from "@/funds/types";
import type { Transaction } from "@/transactions/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Represents reference data required for the transaction workspace, including accounting periods, accounts, funds, and goals.
 */
interface TransactionWorkspaceReferenceData {
  readonly openAccountingPeriods: AccountingPeriod[];
  readonly allAccountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
}

/**
 * Represents the data required for displaying the transaction workspace, including reference data and the list of transactions.
 */
interface TransactionWorkspaceListData extends TransactionWorkspaceReferenceData {
  readonly currentPage: number;
  readonly normalizedAccountingPeriodIds: string[];
  readonly normalizedAccountIds: string[];
  readonly normalizedFundIds: string[];
  readonly transactions: {
    readonly items: Transaction[];
    readonly totalCount: number;
  };
}

/**
 * Fetches reference data required for the transaction workspace, including accounting periods, accounts, funds, and goals.
 */
const getTransactionWorkspaceReferenceData =
  async function (): Promise<TransactionWorkspaceReferenceData> {
    const apiClient = createApiClient();

    const allAccountingPeriodsPromise = apiClient.GET("/accounting-periods", {
      params: {
        query: {
          Limit: 500,
        },
      },
    });
    const accountsPromise = apiClient.GET("/accounts/with-balances");
    const fundsPromise = apiClient.GET("/funds/with-balances");

    const [allAccountingPeriodsResponse, accountsResponse, fundsResponse] =
      await Promise.all([
        allAccountingPeriodsPromise,
        accountsPromise,
        fundsPromise,
      ]);

    const allAccountingPeriods = unwrapApiResponse(
      allAccountingPeriodsResponse,
      "Failed to fetch accounting periods",
    );
    const accounts = unwrapApiResponse(
      accountsResponse,
      "Failed to fetch accounts",
    );
    const funds = unwrapApiResponse(fundsResponse, "Failed to fetch funds");

    const openAccountingPeriods = allAccountingPeriods.items.filter(
      (period) => period.isOpen,
    );
    let assignmentGoals: AssignmentGoal[] = [];
    let spendingGoals: SpendingGoal[] = [];

    if (openAccountingPeriods.length > 0) {
      const [assignmentGoalResponse, spendingGoalResponse] = await Promise.all([
        apiClient.GET("/goals/assignment", {
          params: {
            query: {
              "Filter.AccountingPeriodIds": openAccountingPeriods.map(
                (period) => period.id,
              ),
            },
          },
        }),
        apiClient.GET("/goals/spending", {
          params: {
            query: {
              "Filter.AccountingPeriodIds": openAccountingPeriods.map(
                (period) => period.id,
              ),
            },
          },
        }),
      ]);

      assignmentGoals = unwrapApiResponse(
        assignmentGoalResponse,
        "Failed to fetch assignment goals",
      ).items;
      spendingGoals = unwrapApiResponse(
        spendingGoalResponse,
        "Failed to fetch spending goals",
      ).items;
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
  const apiClient = createApiClient();
  const response = await apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId,
      },
    },
  });

  if (response.response.status === 404) {
    return null;
  }

  return unwrapApiResponse(response, "Failed to fetch the transaction");
};

/**
 * Fetches the list of transactions within the workspace based on search parameters.
 */
const getTransactionWorkspaceListData = async function (
  searchParams: TransactionWorkspaceSearchParams,
): Promise<TransactionWorkspaceListData> {
  const { accountingPeriodIds, accountIds, fundIds, sort, page } = searchParams;
  const apiClient = createApiClient();
  const currentPage = normalizePageValue(page);
  const normalizedAccountingPeriodIds = normalizeStringSearchParams(
    toRepeatedSearchParams(accountingPeriodIds),
  );
  const normalizedAccountIds = normalizeStringSearchParams(
    toRepeatedSearchParams(accountIds),
  );
  const normalizedFundIds = normalizeStringSearchParams(
    toRepeatedSearchParams(fundIds),
  );

  const transactionsPromise = apiClient.GET("/transactions", {
    params: {
      query: {
        ...(normalizedAccountingPeriodIds.length > 0
          ? { "Filter.AccountingPeriodIds": normalizedAccountingPeriodIds }
          : {}),
        ...(normalizedAccountIds.length > 0
          ? { "Filter.AccountIds": normalizedAccountIds }
          : {}),
        ...(normalizedFundIds.length > 0
          ? { "Filter.FundIds": normalizedFundIds }
          : {}),
        Sort: sort ?? null,
        Limit: rowsPerPage,
        Offset: getPageOffset(currentPage),
      },
    },
  });

  const [referenceData, transactionsResponse] = await Promise.all([
    getTransactionWorkspaceReferenceData(),
    transactionsPromise,
  ]);

  const transactions = unwrapApiResponse(
    transactionsResponse,
    "Failed to fetch transactions",
  );

  return {
    ...referenceData,
    currentPage,
    normalizedAccountingPeriodIds,
    normalizedAccountIds,
    normalizedFundIds,
    transactions,
  };
};

export {
  getTransactionById,
  getTransactionWorkspaceListData,
  getTransactionWorkspaceReferenceData,
};
