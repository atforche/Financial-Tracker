import type {
  AccountingPeriod,
  AccountingPeriodWithExpectedIncomeSources,
  ExpectedIncomeSourcesByPeriod,
} from "@/accounting-periods/types";
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
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import type { Location } from "@/locations/types";
import type { Transaction } from "@/transactions/types";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import createApiClient from "@/framework/data/createApiClient";
import { normalizeAccountTypes } from "@/accounts/accountTypeFilterHelpers";
import { normalizeFullyPosted } from "@/transactions/fullyPostedFilter";
import { normalizeTransactionTypes } from "@/transactions/transactionTypeFilter";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Represents the accounts, funds, and locations that transaction pages choose from, with accounting periods.
 */
interface TransactionChoices {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly locations: Location[];
}

/**
 * Represents reference data required by the transaction forms, which can only enter transactions in open periods.
 */
interface TransactionWorkspaceReferenceData extends Omit<
  TransactionChoices,
  "accountingPeriods"
> {
  readonly openAccountingPeriods: AccountingPeriod[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly expectedIncomeSources: ExpectedIncomeSourcesByPeriod;
}

/**
 * Represents the data required for displaying the transaction workspace, including its filter choices and the list of
 * transactions.
 */
interface TransactionWorkspaceListData extends TransactionChoices {
  readonly currentPage: number;
  readonly normalizedAccountingPeriodIds: string[];
  readonly normalizedAccountIds: string[];
  readonly normalizedFundIds: string[];
  readonly selectedAccountIds: string[];
  readonly selectedFundIds: string[];
  readonly transactions: {
    readonly items: Transaction[];
    readonly totalCount: number;
  };
}

/**
 * Represents the reference data required for a Transaction detail page.
 */
interface TransactionWorkspaceDetailReferenceData {
  readonly accountingPeriod: AccountingPeriodWithExpectedIncomeSources;
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
}

/**
 * Fetches the choices shared by transaction pages, with every accounting period or only the open ones.
 */
const getTransactionChoices = async function (
  apiClient: Awaited<ReturnType<typeof createApiClient>>,
  onlyOpenPeriods: boolean,
): Promise<TransactionChoices> {
  const [accountingPeriods, accounts, funds, locations] = await Promise.all([
    apiClient.GET("/accounting-periods", {
      params: {
        query: onlyOpenPeriods ? { "Filter.IsOpen": true } : { Limit: 500 },
      },
    }),
    apiClient.GET("/accounts/with-balances"),
    apiClient.GET("/funds/with-balances"),
    apiClient.GET("/locations", { params: { query: { Limit: 500 } } }),
  ]);
  return {
    accountingPeriods: unwrapApiResponse(
      accountingPeriods,
      "Failed to fetch accounting periods",
    ).items,
    accounts: unwrapApiResponse(accounts, "Failed to fetch accounts").items,
    funds: unwrapApiResponse(funds, "Failed to fetch funds").items,
    locations: unwrapApiResponse(locations, "Failed to fetch Locations").items,
  };
};

/**
 * Fetches the Fund Goals of the provided accounting periods with their progress.
 */
const getFundGoalsWithProgress = async function (
  apiClient: Awaited<ReturnType<typeof createApiClient>>,
  accountingPeriodIds: string[],
): Promise<FundGoalWithProgress[]> {
  const [goalResponse, ...progressResponses] = await Promise.all([
    apiClient.GET("/fund-goals", {
      params: { query: { "Filter.AccountingPeriodIds": accountingPeriodIds } },
    }),
    ...accountingPeriodIds.map(async (accountingPeriodId) =>
      apiClient.GET("/fund-goals/progress/{accountingPeriodId}", {
        params: { path: { accountingPeriodId } },
      }),
    ),
  ]);
  const progressByFundGoalId = new Map(
    progressResponses
      .flatMap((response) =>
        unwrapApiResponse(response, "Failed to fetch Fund Goal progress"),
      )
      .map((result) => [result.fundGoalId, result.progress]),
  );
  return unwrapApiResponse(
    goalResponse,
    "Failed to fetch fund goals",
  ).items.flatMap((fundGoal) => {
    const progress = progressByFundGoalId.get(fundGoal.id);
    return typeof progress === "undefined" ? [] : [{ ...fundGoal, progress }];
  });
};

/**
 * Fetches reference data required by the transaction forms.
 */
const getTransactionWorkspaceReferenceData =
  async function (): Promise<TransactionWorkspaceReferenceData> {
    const apiClient = await createApiClient();
    const { accountingPeriods, ...choices } = await getTransactionChoices(
      apiClient,
      true,
    );
    const openPeriodIds = accountingPeriods.map((period) => period.id);
    if (openPeriodIds.length === 0) {
      return {
        ...choices,
        openAccountingPeriods: [],
        fundGoals: [],
        expectedIncomeSources: {},
      };
    }
    // Income can only be entered in open periods, so only their Expected Income Sources can be chosen
    const [expectedIncomeResponse, fundGoals] = await Promise.all([
      apiClient.GET("/accounting-periods/expected-income-sources", {
        params: { query: { accountingPeriodIds: openPeriodIds } },
      }),
      getFundGoalsWithProgress(apiClient, openPeriodIds),
    ]);
    return {
      ...choices,
      openAccountingPeriods: accountingPeriods,
      fundGoals,
      expectedIncomeSources: Object.fromEntries(
        unwrapApiResponse(
          expectedIncomeResponse,
          "Failed to fetch expected income sources",
        ).map((period) => [period.id, period.expectedIncomeSources]),
      ),
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
    "/accounting-periods/expected-income-sources",
    {
      params: { query: { accountingPeriodIds: [accountingPeriodId] } },
    },
  );
  const [accountingPeriodResponse, fundsResponse, fundGoals] =
    await Promise.all([
      accountingPeriodPromise,
      apiClient.GET("/funds/with-balances"),
      getFundGoalsWithProgress(apiClient, [accountingPeriodId]),
    ]);
  const [accountingPeriod] = unwrapApiResponse(
    accountingPeriodResponse,
    "Failed to fetch accounting period",
  );
  if (accountingPeriod === undefined) {
    throw new Error("Failed to find the transaction accounting period");
  }

  return {
    accountingPeriod,
    funds: unwrapApiResponse(fundsResponse, "Failed to fetch funds").items,
    fundGoals,
  };
};

/**
 * Fetches the list of transactions within the workspace based on search parameters.
 */
const getTransactionWorkspaceListData = async function (
  searchParams: TransactionWorkspaceSearchParams,
  loadTransactions = true,
): Promise<TransactionWorkspaceListData> {
  const {
    accountingPeriodIds,
    accountIds,
    fundIds,
    locationIds,
    fundNames,
    accountTypes,
    accountNames,
    transactionTypes,
    fullyPosted,
    startDate,
    endDate,
    startAccountingPeriodId,
    endAccountingPeriodId,
    sort,
    page,
    pageSize,
  } = searchParams;
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
  const normalizedLocationIds = normalizeStringSearchParams(
    toRepeatedSearchParams(locationIds),
  );
  const normalizedFundNames = normalizeStringSearchParams(
    toRepeatedSearchParams(fundNames),
    (fundName) => fundName.toLocaleLowerCase(),
  );
  const normalizedAccountTypes = normalizeAccountTypes(
    toRepeatedSearchParams(accountTypes),
  );
  const normalizedAccountNames = normalizeStringSearchParams(
    toRepeatedSearchParams(accountNames),
  );
  const normalizedTransactionTypes = normalizeTransactionTypes(
    toRepeatedSearchParams(transactionTypes),
  );
  const normalizedFullyPosted = normalizeFullyPosted(fullyPosted);
  const choices = await getTransactionChoices(apiClient, false);
  const hasDerivedAccountFilter =
    normalizedAccountTypes.length > 0 || normalizedAccountNames.length > 0;
  const selectedAccountIds = choices.accounts
    .filter(
      (account) =>
        (normalizedAccountIds.length === 0 ||
          normalizedAccountIds.includes(account.id)) &&
        (normalizedAccountTypes.length === 0 ||
          normalizedAccountTypes.includes(account.type)) &&
        (normalizedAccountNames.length === 0 ||
          normalizedAccountNames.includes(account.name)),
    )
    .map((account) => account.id);
  const hasDerivedFundFilter = normalizedFundNames.length > 0;
  const selectedFundIds = choices.funds
    .filter(
      (fund) =>
        (normalizedFundIds.length === 0 ||
          normalizedFundIds.includes(fund.id)) &&
        (normalizedFundNames.length === 0 ||
          normalizedFundNames.includes(fund.name.toLocaleLowerCase())),
    )
    .map((fund) => fund.id);
  const hasAccountFilter =
    normalizedAccountIds.length > 0 || hasDerivedAccountFilter;
  const hasFundFilter = normalizedFundIds.length > 0 || hasDerivedFundFilter;
  const query = {
    ...(normalizedAccountingPeriodIds.length > 0
      ? { "Filter.AccountingPeriodIds": normalizedAccountingPeriodIds }
      : {}),
    ...(hasAccountFilter ? { "Filter.AccountIds": selectedAccountIds } : {}),
    ...(hasFundFilter ? { "Filter.FundIds": selectedFundIds } : {}),
    ...(normalizedLocationIds.length > 0
      ? { "Filter.LocationIds": normalizedLocationIds }
      : {}),
    ...(normalizedTransactionTypes.length > 0
      ? { "Filter.Types": [...normalizedTransactionTypes] }
      : {}),
    ...(normalizedFullyPosted === null
      ? {}
      : { "Filter.FullyPosted": normalizedFullyPosted === "true" }),
    Sort: sort ?? null,
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage, rowsPerPage),
  };
  const transactions =
    !loadTransactions ||
    (hasAccountFilter && selectedAccountIds.length === 0) ||
    (hasFundFilter && selectedFundIds.length === 0)
      ? { items: [], totalCount: 0 }
      : await (async function (): Promise<{
          items: Transaction[];
          totalCount: number;
        }> {
          if (
            typeof startDate !== "undefined" ||
            typeof endDate !== "undefined"
          ) {
            return unwrapApiResponse(
              await apiClient.GET("/transactions/date-range", {
                params: {
                  query: {
                    ...query,
                    ...(startDate === undefined
                      ? {}
                      : { "Range.Start": startDate }),
                    ...(endDate === undefined ? {} : { "Range.End": endDate }),
                  },
                },
              }),
              "Failed to fetch transactions",
            ).transactions;
          }
          if (
            typeof startAccountingPeriodId !== "undefined" &&
            typeof endAccountingPeriodId !== "undefined"
          ) {
            return unwrapApiResponse(
              await apiClient.GET("/transactions/accounting-period-range", {
                params: {
                  query: {
                    ...query,
                    "Range.Start": startAccountingPeriodId,
                    "Range.End": endAccountingPeriodId,
                  },
                },
              }),
              "Failed to fetch transactions",
            ).transactions;
          }
          return unwrapApiResponse(
            await apiClient.GET("/transactions", { params: { query } }),
            "Failed to fetch transactions",
          );
        })();

  return {
    ...choices,
    currentPage,
    normalizedAccountingPeriodIds,
    normalizedAccountIds,
    normalizedFundIds,
    selectedAccountIds: hasAccountFilter ? selectedAccountIds : [],
    selectedFundIds: hasFundFilter ? selectedFundIds : [],
    transactions,
  };
};

export {
  getTransactionById,
  getTransactionWorkspaceDetailReferenceData,
  getTransactionWorkspaceListData,
  getTransactionWorkspaceReferenceData,
};
