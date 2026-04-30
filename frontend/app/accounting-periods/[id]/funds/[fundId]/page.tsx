import { Button, Stack, Typography } from "@mui/material";
import routes, { routeBreadcrumbs, withQuery } from "@/framework/routes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import CurrentGoalFrame from "@/app/accounting-periods/[id]/funds/[fundId]/CurrentGoalFrame";
import type { FundTransactionSortOrder } from "@/data/fundTypes";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/app/funds/[id]/TransactionListFrame";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
    fundId: string;
  }>;
  readonly searchParams: Promise<{
    transactionSearch?: string;
    transactionSort?: FundTransactionSortOrder;
  }>;
}

/**
 * Component that displays the view for a single fund in the context of an accounting period.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id, fundId } = await params;
  const { transactionSearch, transactionSort } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );
  const fundPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/funds/{fundId}",
    {
      params: {
        path: {
          fundId,
          accountingPeriodId: id,
        },
      },
    },
  );
  const goalPromise = apiClient.GET("/goals", {
    body: {
      accountingPeriodId: id,
      fundId,
    },
  });
  const fundTransactionsPromise = apiClient.GET(
    "/funds/{fundId}/transactions",
    {
      params: {
        path: {
          fundId,
        },
        query: {
          AccountingPeriodId: id,
          Search: transactionSearch ?? "",
          Sort: transactionSort ?? null,
        },
      },
    },
  );
  const [
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: fundData, error: fundError },
    { data: goalData },
    { data: fundTransactionsData, error: fundTransactionsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    goalPromise,
    fundTransactionsPromise,
  ]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof fundData === "undefined" ||
    typeof fundTransactionsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch fund with ID ${fundId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? fundError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  const isSystemFund = fundData.name === "Unassigned";
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={routeBreadcrumbs.accountingPeriods.fundDetail(
            {
              id,
              name: accountingPeriodData.name,
            },
            {
              id: fundId,
              name: fundData.name,
            },
          )}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={withQuery(routes.funds.update(fundId), {
              accountingPeriodId: id,
            })}
            disabled={isSystemFund}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={withQuery(routes.funds.delete(fundId), {
              accountingPeriodId: id,
            })}
            disabled={isSystemFund}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={fundData.name} />
        <CaptionedValue caption="Description" value={fundData.description} />
      </CaptionedFrame>
      <CaptionedFrame caption="Balance">
        <CaptionedValue
          caption="Opening Balance"
          value={formatCurrency(fundData.openingBalance)}
        />
        <CaptionedValue
          caption="Amount Assigned"
          value={
            <span style={{ color: "green" }}>
              + {formatCurrency(fundData.amountAssigned)}
            </span>
          }
        />
        <CaptionedValue
          caption="Amount Spent"
          value={
            <span style={{ color: "red" }}>
              - {formatCurrency(fundData.amountSpent)}
            </span>
          }
        />
        <CaptionedValue
          caption="Closing Balance"
          value={formatCurrency(fundData.closingBalance)}
        />
      </CaptionedFrame>
      <CurrentGoalFrame
        fundId={fundData.id}
        goal={goalData ?? null}
        accountingPeriodId={id}
        isAccountingPeriodOpen={accountingPeriodData.isOpen}
        isSystemFund={isSystemFund}
      />
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar paramName="transactionSearch" />
        <TransactionListFrame
          fund={fundData}
          accountingPeriodId={id}
          data={fundTransactionsData.items}
          totalCount={fundTransactionsData.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export default Page;
