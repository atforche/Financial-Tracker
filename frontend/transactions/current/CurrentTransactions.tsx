import { Button, Stack } from "@mui/material";
import type {
  CurrentTransactions as CurrentTransactionsModel,
  TransactionSortOrder,
  TransactionType,
} from "@/transactions/types";
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
import CurrentTransactionsByTypeCard from "@/transactions/current/CurrentTransactionsByTypeCard";
import CurrentTransactionsFilter from "@/transactions/current/CurrentTransactionsFilter";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the CurrentTransactions component.
 */
interface CurrentTransactionsSearchParams {
  transactionType?: TransactionType | readonly TransactionType[];
  accountName?: string | readonly string[];
  fundName?: string | readonly string[];
  unpostedTransactionSort?: TransactionSortOrder;
  unpostedTransactionPage?: number | string | null;
  postedTransactionSort?: TransactionSortOrder;
  postedTransactionPage?: number | string | null;
}

/**
 * Props for the CurrentTransactions component.
 */
interface CurrentTransactionsProps {
  readonly searchParams: Promise<CurrentTransactionsSearchParams>;
}

const createEmptyCurrent = function (): CurrentTransactionsModel {
  return {
    accountingPeriodId: null,
    accountingPeriodName: null,
    availableAccountNames: [],
    availableFundNames: [],
    transactionTypes: [],
    unpostedTransactions: {
      items: [],
      totalCount: 0,
    },
    postedTransactions: {
      items: [],
      totalCount: 0,
    },
  };
};

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
    Array.isArray(transactionType)
      ? transactionType
      : typeof transactionType === "string"
        ? [transactionType]
        : [],
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    Array.isArray(accountName)
      ? accountName
      : typeof accountName === "string"
        ? [accountName]
        : [],
  );
  const currentFundNames = normalizeRequestedFundNames(
    Array.isArray(fundName)
      ? fundName
      : typeof fundName === "string"
        ? [fundName]
        : [],
  );

  const apiClient = getApiClient();
  const current: CurrentTransactionsModel =
    (
      await apiClient.GET("/transactions/current", {
        params: {
          query: {
            ...(shouldPersistTransactionTypes(currentTransactionTypes)
              ? { TransactionType: [...currentTransactionTypes] }
              : {}),
            ...(shouldPersistAccountNames(currentAccountNames)
              ? { AccountName: [...currentAccountNames] }
              : {}),
            ...(shouldPersistFundNames(currentFundNames)
              ? { FundName: [...currentFundNames] }
              : {}),
            ...(typeof unpostedTransactionSort === "string"
              ? { UnpostedTransactionSort: unpostedTransactionSort }
              : {}),
            UnpostedTransactionLimit: rowsPerPage,
            UnpostedTransactionOffset: getPageOffset(
              normalizePageValue(unpostedTransactionPage),
            ),
            ...(typeof postedTransactionSort === "string"
              ? { PostedTransactionSort: postedTransactionSort }
              : {}),
            PostedTransactionLimit: rowsPerPage,
            PostedTransactionOffset: getPageOffset(
              normalizePageValue(postedTransactionPage),
            ),
          },
        },
      })
    ).data ?? createEmptyCurrent();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <CurrentTransactionsFilter
          availableAccountNames={current.availableAccountNames}
          availableFundNames={current.availableFundNames}
        />
      </Stack>
      <CurrentTransactionsByTypeCard current={current} />
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
            <Button
              variant="contained"
              href={routes.workspace({ action: "create" })}
            >
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
