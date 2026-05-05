import type {
  AccountingPeriodAccountSortOrder,
  AccountingPeriodFundSortOrder,
  AccountingPeriodGoalSortOrder,
  AccountingPeriodTransactionSortOrder,
} from "@/accounting-periods/types";
import AccountingPeriodViewListFrames, {
  type ToggleState,
} from "@/accounting-periods/detail/AccountingPeriodDetailViewListFrames";
import { Button, Stack } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import routes from "@/accounting-periods/routes";

/**
 * Parameters for the AccountingPeriodDetailView component.
 */
interface AccountingPeriodDetailViewParams {
  id: string;
}

/**
 * Search parameters for the AccountingPeriodDetailView component.
 */
interface AccountingPeriodDetailViewSearchParams {
  search?: string;
  fundSort?: AccountingPeriodFundSortOrder;
  goalSort?: AccountingPeriodGoalSortOrder;
  accountSort?: AccountingPeriodAccountSortOrder;
  transactionSort?: AccountingPeriodTransactionSortOrder;
  page?: number;
  display?: ToggleState;
}

/**
 * Props for the AccountingPeriodDetailView component.
 */
interface AccountingPeriodDetailViewProps {
  readonly params: Promise<AccountingPeriodDetailViewParams>;
  readonly searchParams: Promise<AccountingPeriodDetailViewSearchParams>;
}

/**
 * Component that displays the view for a single accounting period.
 */
const AccountingPeriodDetailView = async function ({
  params,
  searchParams,
}: AccountingPeriodDetailViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { search, fundSort, goalSort, accountSort, transactionSort } =
    await searchParams;

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
    "/accounting-periods/{accountingPeriodId}/funds",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
        query: {
          Search: search ?? "",
          Sort: fundSort ?? null,
        },
      },
    },
  );
  const goalPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/goals",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
        query: {
          Search: search ?? "",
          Sort: goalSort ?? null,
        },
      },
    },
  );
  const accountPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/accounts",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
        query: {
          Search: search ?? "",
          Sort: accountSort ?? null,
        },
      },
    },
  );
  const transactionPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/transactions",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
        query: {
          Search: search ?? "",
          Sort: transactionSort ?? null,
        },
      },
    },
  );
  const [
    { data: accountingPeriod, error },
    { data: funds, error: fundError },
    { data: goals, error: goalError },
    { data: accounts, error: accountError },
    { data: transactions, error: transactionError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    goalPromise,
    accountPromise,
    transactionPromise,
  ]);
  if (
    typeof accountingPeriod === "undefined" ||
    typeof funds === "undefined" ||
    typeof goals === "undefined" ||
    typeof accounts === "undefined" ||
    typeof transactions === "undefined"
  ) {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${error?.detail ?? fundError?.detail ?? goalError?.detail ?? accountError?.detail ?? transactionError?.detail}`,
    );
  }

  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs breadcrumbs={breadcrumbs.detail(accountingPeriod)} />
        <Stack direction="row" spacing={1}>
          {accountingPeriod.isOpen ? (
            <Button
              variant="contained"
              color="primary"
              href={routes.close({ id })}
            >
              Close
            </Button>
          ) : (
            <Button
              variant="contained"
              color="primary"
              href={routes.reopen({ id })}
            >
              Reopen
            </Button>
          )}
          <Button
            variant="contained"
            color="error"
            href={routes.delete({ id })}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={accountingPeriod.name} />
        <CaptionedValue
          caption="Is Open"
          value={accountingPeriod.isOpen ? "Yes" : "No"}
        />
        <CaptionedValue
          caption="Opening Balance"
          value={formatCurrency(accountingPeriod.openingBalance)}
        />
        <CaptionedValue
          caption="Closing Balance"
          value={formatCurrency(accountingPeriod.closingBalance)}
        />
      </CaptionedFrame>
      <AccountingPeriodViewListFrames
        accountingPeriod={accountingPeriod}
        funds={funds.items}
        fundTotalCount={funds.totalCount}
        goals={goals.items}
        goalTotalCount={goals.totalCount}
        accounts={accounts.items}
        accountTotalCount={accounts.totalCount}
        transactions={transactions.items}
        transactionTotalCount={transactions.totalCount}
      />
    </Stack>
  );
};

export type {
  AccountingPeriodDetailViewParams,
  AccountingPeriodDetailViewSearchParams,
};
export default AccountingPeriodDetailView;
