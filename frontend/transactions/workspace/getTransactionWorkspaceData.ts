import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import {
  normalizeStringSearchParams,
  toRepeatedSearchParams,
} from "@/framework/routes/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import type { Transaction } from "@/transactions/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Represents reference data required for the transaction workspace, including accounting periods, accounts, funds, and Fund Goals.
 */
interface TransactionWorkspaceReferenceData {
  readonly openAccountingPeriods: AccountingPeriod[];
  readonly allAccountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
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
 * Represents the reference data required for a Transaction detail page.
 */
interface TransactionWorkspaceDetailReferenceData {
  readonly accountingPeriod: AccountingPeriod;
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
}

/**
 * Fetches reference data required for the transaction workspace, including accounting periods, accounts, funds, and Fund Goals.
 */
const getTransactionWorkspaceReferenceData =
  async function (): Promise<TransactionWorkspaceReferenceData> {
    const apiClient = await createApiClient();

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
    let fundGoals: FundGoalWithProgress[] = [];

    if (openAccountingPeriods.length > 0) {
      const goalResponse = await apiClient.GET("/fund-goals", {
        params: {
          query: {
            "Filter.AccountingPeriodIds": openAccountingPeriods.map(
              (period) => period.id,
            ),
            Limit: 500,
          },
        },
      });
      const loadedFundGoals = unwrapApiResponse(
        goalResponse,
        "Failed to fetch fund goals",
      ).items;
      const progressResults = (
        await Promise.all(
          openAccountingPeriods.map(async (accountingPeriod) =>
            unwrapApiResponse(
              await apiClient.GET("/fund-goals/progress/{accountingPeriodId}", {
                params: { path: { accountingPeriodId: accountingPeriod.id } },
              }),
              "Failed to fetch Fund Goal progress",
            ),
          ),
        )
      ).flatMap((results) => results);
      const progressByFundGoalId = new Map(
        progressResults.map((result) => [result.fundGoalId, result]),
      );
      fundGoals = loadedFundGoals.flatMap((fundGoal) => {
        const result = progressByFundGoalId.get(fundGoal.id);
        return typeof result === "undefined"
          ? []
          : [{ ...fundGoal, progress: result.progress }];
      });
    }

    return {
      openAccountingPeriods,
      allAccountingPeriods: allAccountingPeriods.items,
      accounts: accounts.items,
      funds: funds.items,
      fundGoals,
    };
  };

/**
 * Fetches a transaction by its ID.
 */
const getTransactionById = async function (
  transactionId: string,
): Promise<Transaction | null> {
  const apiClient = await createApiClient();
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
 * Fetches only the reference data required for a Transaction detail page.
 */
const getTransactionWorkspaceDetailReferenceData = async function (
  accountingPeriodId: string,
): Promise<TransactionWorkspaceDetailReferenceData> {
  const apiClient = await createApiClient();
  const accountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: { path: { accountingPeriodId } },
    },
  );
  const fundsPromise = apiClient.GET("/funds/with-balances");
  const fundGoalsPromise = apiClient.GET("/fund-goals", {
    params: {
      query: {
        "Filter.AccountingPeriodIds": [accountingPeriodId],
        Limit: 500,
      },
    },
  });
  const progressPromise = apiClient.GET(
    "/fund-goals/progress/{accountingPeriodId}",
    {
      params: { path: { accountingPeriodId } },
    },
  );
  const [
    accountingPeriodResponse,
    fundsResponse,
    fundGoalsResponse,
    progressResponse,
  ] = await Promise.all([
    accountingPeriodPromise,
    fundsPromise,
    fundGoalsPromise,
    progressPromise,
  ]);
  const progressByFundGoalId = new Map(
    unwrapApiResponse(
      progressResponse,
      "Failed to fetch Fund Goal progress",
    ).map((result) => [result.fundGoalId, result.progress]),
  );

  return {
    accountingPeriod: unwrapApiResponse(
      accountingPeriodResponse,
      "Failed to fetch accounting period",
    ),
    funds: unwrapApiResponse(fundsResponse, "Failed to fetch funds").items,
    fundGoals: unwrapApiResponse(
      fundGoalsResponse,
      "Failed to fetch fund goals",
    ).items.flatMap((fundGoal) => {
      const progress = progressByFundGoalId.get(fundGoal.id);
      return typeof progress === "undefined" ? [] : [{ ...fundGoal, progress }];
    }),
  };
};

/**
 * Fetches the list of transactions within the workspace based on search parameters.
 */
const getTransactionWorkspaceListData = async function (
  searchParams: TransactionWorkspaceSearchParams,
): Promise<TransactionWorkspaceListData> {
  const { accountingPeriodIds, accountIds, fundIds, sort, page, pageSize } =
    searchParams;
  const apiClient = await createApiClient();
  const currentPage = normalizePageValue(page);
  const rowsPerPage = getRowsPerPage(pageSize);
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
        Offset: getPageOffset(currentPage, rowsPerPage),
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
  getTransactionWorkspaceDetailReferenceData,
  getTransactionWorkspaceListData,
  getTransactionWorkspaceReferenceData,
};
