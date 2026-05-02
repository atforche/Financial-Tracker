import { Button, Stack, Typography } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import CurrentGoalFrame from "@/accounting-periods/funds/CurrentGoalFrame";
import type { FundTransactionSortOrder } from "@/funds/types";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/funds/FundTransactionListFrame";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import fundRoutes from "@/funds/routes";
import getApiClient from "@/framework/data/getApiClient";
import nameof from "@/framework/data/nameof";

/**
 * Parameters for the AccountingPeriodFundView component.
 */
interface AccountingPeriodFundViewParams {
  id: string;
  fundId: string;
}

/**
 * Search parameters for the AccountingPeriodFundView component.
 */
interface AccountingPeriodFundViewSearchParams {
  search?: string;
  sort?: FundTransactionSortOrder;
}

/**
 * Props for the AccountingPeriodFundView component.
 */
interface AccountingPeriodFundViewProps {
  readonly params: Promise<AccountingPeriodFundViewParams>;
  readonly searchParams: Promise<AccountingPeriodFundViewSearchParams>;
}

/**
 * Component that displays the view for a single fund in the context of an accounting period.
 */
const AccountingPeriodFundView = async function ({
  params,
  searchParams,
}: AccountingPeriodFundViewProps): Promise<JSX.Element> {
  const { id, fundId } = await params;
  const { search, sort } = await searchParams;

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
          Search: search ?? "",
          Sort: sort ?? null,
        },
      },
    },
  );
  const [
    { data: accountingPeriod, error: accountingPeriodError },
    { data: fund, error: fundError },
    { data: goal },
    { data: transactions, error: fundTransactionsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    goalPromise,
    fundTransactionsPromise,
  ]);

  if (
    typeof accountingPeriod === "undefined" ||
    typeof fund === "undefined" ||
    typeof transactions === "undefined"
  ) {
    throw new Error(
      `Failed to fetch fund with ID ${fundId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? fundError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  const isSystemFund = fund.name === "Unassigned";
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={breadcrumbs.fundDetail(accountingPeriod, fund)}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={fundRoutes.update({ id: fundId }, { accountingPeriodId: id })}
            disabled={isSystemFund}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={fundRoutes.delete({ id: fundId }, { accountingPeriodId: id })}
            disabled={isSystemFund}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={fund.name} />
        <CaptionedValue caption="Description" value={fund.description} />
      </CaptionedFrame>
      <CaptionedFrame caption="Balance">
        <CaptionedValue
          caption="Opening Balance"
          value={formatCurrency(fund.openingBalance)}
        />
        <CaptionedValue
          caption="Amount Assigned"
          value={
            <span style={{ color: "green" }}>
              + {formatCurrency(fund.amountAssigned)}
            </span>
          }
        />
        <CaptionedValue
          caption="Amount Spent"
          value={
            <span style={{ color: "red" }}>
              - {formatCurrency(fund.amountSpent)}
            </span>
          }
        />
        <CaptionedValue
          caption="Closing Balance"
          value={formatCurrency(fund.closingBalance)}
        />
      </CaptionedFrame>
      <CurrentGoalFrame
        fundId={fund.id}
        goal={goal ?? null}
        accountingPeriodId={id}
        isAccountingPeriodOpen={accountingPeriod.isOpen}
        isSystemFund={isSystemFund}
      />
      <Stack spacing={2} style={{ maxWidth: 1000 }}>
        <Typography variant="h6">Transactions</Typography>
        <SearchBar
          paramName={nameof<AccountingPeriodFundViewSearchParams>("search")}
        />
        <TransactionListFrame
          fund={fund}
          data={transactions.items}
          totalCount={transactions.totalCount}
        />
      </Stack>
    </Stack>
  );
};

export type {
  AccountingPeriodFundViewParams,
  AccountingPeriodFundViewSearchParams,
};
export default AccountingPeriodFundView;
