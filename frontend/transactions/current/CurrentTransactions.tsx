import { Box, Button, Stack, Typography } from "@mui/material";
import type {
  CurrentTransactions as CurrentTransactionsModel,
  TransactionSortOrder,
} from "@/transactions/types";
import { getPageOffset, normalizePageValue } from "@/framework/listframe/page";
import CurrentTransactionListFrame from "@/transactions/current/CurrentTransactionListFrame";
import CurrentTransactionsByTypeCard from "@/transactions/current/CurrentTransactionsByTypeCard";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";
import routes from "@/transactions/routes";
import { rowsPerPage } from "@/framework/listframe/Constants";

/**
 * Search parameters for the CurrentTransactions component.
 */
interface CurrentTransactionsSearchParams {
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
    unpostedTransactionSort,
    unpostedTransactionPage,
    postedTransactionSort,
    postedTransactionPage,
  } = await searchParams;

  const apiClient = getApiClient();
  const current: CurrentTransactionsModel =
    (
      await apiClient.GET("/transactions/current", {
        params: {
          query: {
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
      <Box
        sx={{
          maxWidth: 1440,
          width: "100%",
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          px: { xs: 2, md: 3 },
          py: { xs: 2.5, md: 3 },
          background:
            "linear-gradient(135deg, rgba(2,132,199,0.10) 0%, rgba(255,255,255,0.97) 46%, rgba(249,115,22,0.08) 100%)",
        }}
      >
        <Stack spacing={1}>
          <Typography
            variant="overline"
            sx={{
              color: "text.secondary",
              letterSpacing: 1.4,
              fontWeight: 700,
            }}
          >
            Transactions
          </Typography>
          <Typography variant="h5">Current Transactions</Typography>
          <Typography color="text.secondary">
            {current.accountingPeriodName === null
              ? "No current accounting period is available to show transaction activity yet."
              : `Snapshot of transaction activity for ${current.accountingPeriodName}.`}
          </Typography>
        </Stack>
      </Box>
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
