import { Button, Stack } from "@mui/material";
import type {
  Transaction,
  TransactionSortValue,
  TransactionSummaryByType,
  TransactionType,
} from "@/transactions/transaction";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
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
import CurrentTransactionListFrame from "@/transactions/current/CurrentTransactionListFrame";
import CurrentTransactionsFilter from "@/transactions/current/CurrentTransactionsFilter";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";
import { toRepeatedSearchParam } from "@/framework/routes/helpers";
import {
  getPostableTransactionAccounts,
  getTransactionAccountIds,
  getTransactionFundIds,
} from "@/transactions/postingHelpers";
import TransactionsByTypeCard from "@/transactions/TransactionsByTypeCard";

/**
 * Search parameters for the CurrentTransactions component.
 */
interface CurrentTransactionsSearchParams {
  transactionType?: TransactionType | readonly TransactionType[];
  accountName?: string | readonly string[];
  fundName?: string | readonly string[];
  unpostedTransactionSort?: TransactionSortValue;
  unpostedTransactionPage?: number | string | null;
  postedTransactionSort?: TransactionSortValue;
  postedTransactionPage?: number | string | null;
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
  const [{ data: periods }, { data: accounts }, { data: funds }] =
    await Promise.all([
      apiClient.GET("/accounting-periods", {
        params: { query: { Sort: "DateDescending", Limit: 500 } },
      }),
      apiClient.GET("/accounts"),
      apiClient.GET("/funds"),
    ]);
  const accountingPeriod = periods?.items.find((period) => period.isOpen);
  const availableAccountNames =
    accounts?.items.map((account) => account.name) ?? [];
  const availableFundNames = funds?.items.map((fund) => fund.name) ?? [];
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
    const [{ data: unpostedData }, { data: postedData }] = await Promise.all([
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
    if (
      typeof unpostedData !== "undefined" &&
      typeof postedData !== "undefined"
    ) {
      const accountIds = new Set(
        accounts?.items
          .filter((account) => currentAccountNames.includes(account.name))
          .map((account) => account.id) ?? [],
      );
      const fundIds = new Set(
        funds?.items
          .filter((fund) => currentFundNames.includes(fund.name))
          .map((fund) => fund.id) ?? [],
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
      const unposted = filterTransactions(
        unpostedData.transactions.items,
      ).filter(
        (transaction) => getPostableTransactionAccounts(transaction).length > 0,
      );
      const posted = filterTransactions(postedData.transactions.items).filter(
        (transaction) =>
          getPostableTransactionAccounts(transaction).length === 0,
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
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <CurrentTransactionsFilter
          availableAccountNames={current.availableAccountNames}
          availableFundNames={current.availableFundNames}
        />
      </Stack>
      <TransactionsByTypeCard
        transactionTypes={current.transactionTypes}
      />
      <CurrentTransactionListFrame
        title="Needs Posting"
        description={
          current.accountingPeriodName === null
            ? "Transactions that still need posting will appear here once a current accounting period exists."
            : `Transactions in ${current.accountingPeriodName} that are not fully posted yet.`
        }
        data={current.unpostedTransactions.items}
        totalCount={current.unpostedTransactions.totalCount}
        sortParamName="unpostedTransactionSort"
        pageParamName="unpostedTransactionPage"
        searchParamName="unpostedTransactionSearch"
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
        description={
          current.accountingPeriodName === null
            ? "Fully posted transactions will appear here once a current accounting period exists."
            : `Transactions in ${current.accountingPeriodName} that are fully posted.`
        }
        data={current.postedTransactions.items}
        totalCount={current.postedTransactions.totalCount}
        sortParamName="postedTransactionSort"
        pageParamName="postedTransactionPage"
        searchParamName="postedTransactionSearch"
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
    </Stack>
  );
};

export type { CurrentTransactionsSearchParams };
export default CurrentTransactions;
