import type {
  Transaction,
  TransactionSort,
  TransactionSummaryByType,
  TransactionType,
} from "@/transactions/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import {
  getPostableTransactionAccounts,
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import {
  normalizeRequestedFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
} from "@/transactions/trends/transactionTypeFilter";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import { Button } from "@mui/material";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrentTransactionListFrame from "@/transactions/current/CurrentTransactionListFrame";
import CurrentTransactionsFilter from "@/transactions/current/CurrentTransactionsFilter";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TransactionsByTypeCard from "@/transactions/TransactionsByTypeCard";
import getApiClient from "@/framework/data/getApiClient";
import getApiData from "@/framework/data/apiResponse";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";

/**
 * Search parameters for the CurrentTransactions component.
 */
interface CurrentTransactionsSearchParams {
  transactionType?: TransactionType | readonly TransactionType[];
  accountName?: string | readonly string[];
  fundName?: string | readonly string[];
  unpostedTransactionSort?: TransactionSort;
  unpostedTransactionPage?: number | string | null;
  unpostedTransactionSearch?: string;
  postedTransactionSort?: TransactionSort;
  postedTransactionPage?: number | string | null;
  postedTransactionSearch?: string;
}

/**
 * Props for the CurrentTransactions component.
 */
interface CurrentTransactionsProps {
  readonly searchParams: Promise<CurrentTransactionsSearchParams>;
}

interface CurrentTransactionCollection {
  readonly items: Transaction[];
  readonly totalCount: number;
}

interface CurrentTransactionData {
  readonly accountingPeriodId: string | null;
  readonly accountingPeriodName: string | null;
  readonly availableAccountNames: string[];
  readonly availableFundNames: string[];
  readonly transactionTypes: TransactionSummaryByType[];
  readonly unpostedTransactions: CurrentTransactionCollection;
  readonly postedTransactions: CurrentTransactionCollection;
}

/**
 * Component that displays the current Transactions snapshot.
 */
const CurrentTransactions = async function ({
  searchParams,
}: CurrentTransactionsProps): Promise<JSX.Element> {
  const {
    transactionType,
    accountName,
    fundName,
    unpostedTransactionSort,
    unpostedTransactionPage,
    postedTransactionSort,
    postedTransactionPage,
  } = await searchParams;

  const currentTransactionTypes = normalizeTransactionTypes(
    toRepeatedSearchParam(transactionType),
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    toRepeatedSearchParam(accountName),
  );
  const currentFundNames = normalizeRequestedFundNames(
    toRepeatedSearchParam(fundName),
  );

  const apiClient = getApiClient();
  const [periodsResponse, accountsResponse, fundsResponse] = await Promise.all([
    apiClient.GET("/accounting-periods", {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 500 },
      },
    }),
    apiClient.GET("/accounts"),
    apiClient.GET("/funds"),
  ]);
  const periods = getApiData(
    periodsResponse,
    "Failed to fetch accounting periods",
  );
  const accounts = getApiData(accountsResponse, "Failed to fetch accounts");
  const funds = getApiData(fundsResponse, "Failed to fetch funds");
  const accountingPeriod = periods.items.find((period) => period.isOpen);
  const availableAccountNames = accounts.items.map((account) => account.name);
  const availableFundNames = funds.items.map((fund) => fund.name);
  let current: CurrentTransactionData = {
    accountingPeriodId: accountingPeriod?.id ?? null,
    accountingPeriodName: accountingPeriod?.name ?? null,
    availableAccountNames,
    availableFundNames,
    transactionTypes: [],
    unpostedTransactions: { items: [], totalCount: 0 },
    postedTransactions: { items: [], totalCount: 0 },
  };

  if (typeof accountingPeriod !== "undefined") {
    const [unpostedResponse, postedResponse] = await Promise.all([
      apiClient.GET("/transactions/accounting-period-range", {
        params: {
          query: {
            "Range.Start": accountingPeriod.id,
            "Range.End": accountingPeriod.id,
            "Filter.AccountingPeriodIds": [accountingPeriod.id],
            ...(typeof unpostedTransactionSort === "string"
              ? { Sort: unpostedTransactionSort }
              : {}),
          },
        },
      }),
      apiClient.GET("/transactions/accounting-period-range", {
        params: {
          query: {
            "Range.Start": accountingPeriod.id,
            "Range.End": accountingPeriod.id,
            "Filter.AccountingPeriodIds": [accountingPeriod.id],
            ...(typeof postedTransactionSort === "string"
              ? { Sort: postedTransactionSort }
              : {}),
          },
        },
      }),
    ]);
    const unpostedData = getApiData(
      unpostedResponse,
      "Failed to fetch unposted transactions",
    );
    const postedData = getApiData(
      postedResponse,
      "Failed to fetch posted transactions",
    );
    const accountIds = new Set(
      accounts.items
        .filter((account) => currentAccountNames.includes(account.name))
        .map((account) => account.id),
    );
    const fundIds = new Set(
      funds.items
        .filter((fund) => currentFundNames.includes(fund.name))
        .map((fund) => fund.id),
    );
    const filterTransactions = function (
      transactions: Transaction[],
    ): Transaction[] {
      return transactions.filter((transaction) => {
        if (
          shouldPersistTransactionTypes(currentTransactionTypes) &&
          !currentTransactionTypes.includes(transaction.transactionType)
        ) {
          return false;
        }
        if (
          shouldPersistAccountNames(currentAccountNames) &&
          !getTransactionAccountIds(transaction).some((id) =>
            accountIds.has(id),
          )
        ) {
          return false;
        }
        return (
          !shouldPersistFundNames(currentFundNames) ||
          getTransactionFundIds(transaction).some((id) => fundIds.has(id))
        );
      });
    };
    const unposted = filterTransactions(unpostedData.transactions.items).filter(
      (transaction) => getPostableTransactionAccounts(transaction).length > 0,
    );
    const posted = filterTransactions(postedData.transactions.items).filter(
      (transaction) => getPostableTransactionAccounts(transaction).length === 0,
    );
    const unpostedOffset = getPageOffset(
      normalizePageValue(unpostedTransactionPage),
    );
    const postedOffset = getPageOffset(
      normalizePageValue(postedTransactionPage),
    );
    current = {
      ...current,
      transactionTypes: unpostedData.transactionTypes,
      unpostedTransactions: {
        items: unposted.slice(unpostedOffset, unpostedOffset + rowsPerPage),
        totalCount: unposted.length,
      },
      postedTransactions: {
        items: posted.slice(postedOffset, postedOffset + rowsPerPage),
        totalCount: posted.length,
      },
    };
  }

  return (
    <PageLayout>
      <ConstrainedContent>
        <CurrentTransactionsFilter
          availableAccountNames={current.availableAccountNames}
          availableFundNames={current.availableFundNames}
        />
      </ConstrainedContent>
      <TransactionsByTypeCard transactionTypes={current.transactionTypes} />
      <CurrentTransactionListFrame
        title="Needs Posting"
        data={current.unpostedTransactions.items}
        totalCount={current.unpostedTransactions.totalCount}
        sortParamName={nameof<CurrentTransactionsSearchParams>(
          "unpostedTransactionSort",
        )}
        pageParamName={nameof<CurrentTransactionsSearchParams>(
          "unpostedTransactionPage",
        )}
        searchParamName={nameof<CurrentTransactionsSearchParams>(
          "unpostedTransactionSearch",
        )}
        emptyTitle={
          current.accountingPeriodName === null
            ? "No current accounting period available"
            : "No unposted transactions found"
        }
        emptyDescription={
          current.accountingPeriodName === null
            ? "Create an accounting period to view a current transactions snapshot."
            : `All current-period transactions in ${current.accountingPeriodName} are fully posted.`
        }
        emptyAction={
          current.accountingPeriodName === null ? null : (
            <Button variant="contained" href={routes.workspaceCreate({})}>
              Create transaction
            </Button>
          )
        }
      />
      <CurrentTransactionListFrame
        title="Posted Transactions"
        data={current.postedTransactions.items}
        totalCount={current.postedTransactions.totalCount}
        sortParamName={nameof<CurrentTransactionsSearchParams>(
          "postedTransactionSort",
        )}
        pageParamName={nameof<CurrentTransactionsSearchParams>(
          "postedTransactionPage",
        )}
        searchParamName={nameof<CurrentTransactionsSearchParams>(
          "postedTransactionSearch",
        )}
        emptyTitle={
          current.accountingPeriodName === null
            ? "No current accounting period available"
            : "No posted transactions found"
        }
        emptyDescription={
          current.accountingPeriodName === null
            ? "Create an accounting period to view a current transactions snapshot."
            : `No fully posted transactions are included in ${current.accountingPeriodName} yet.`
        }
      />
    </PageLayout>
  );
};

export type { CurrentTransactionsSearchParams };
export default CurrentTransactions;
