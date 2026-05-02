import type {
  AccountingPeriodAccountSortOrder,
  AccountingPeriodFundSortOrder,
  AccountingPeriodGoalSortOrder,
  AccountingPeriodTransactionSortOrder,
} from "@/accounting-periods/types";
import AccountingPeriodViewListFrames, {
  type ToggleState,
} from "@/accounting-periods/AccountingPeriodViewListFrames";
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
 * Parameters for the AccountingPeriodView component.
 */
interface AccountingPeriodViewParams {
  id: string;
}

/**
 * Search parameters for the AccountingPeriodView component.
 */
interface AccountingPeriodViewSearchParams {
  fundSearch?: string;
  fundSort?: AccountingPeriodFundSortOrder;
  goalSearch?: string;
  goalSort?: AccountingPeriodGoalSortOrder;
  accountSearch?: string;
  accountSort?: AccountingPeriodAccountSortOrder;
  transactionSearch?: string;
  transactionSort?: AccountingPeriodTransactionSortOrder;
  display?: ToggleState;
}

/**
 * Props for the AccountingPeriodView component.
 */
interface AccountingPeriodViewProps {
  readonly params: Promise<AccountingPeriodViewParams>;
  readonly searchParams: Promise<AccountingPeriodViewSearchParams>;
}

/**
 * Component that displays the view for a single accounting period.
 */
const AccountingPeriodView = async function ({
  params,
  searchParams,
}: AccountingPeriodViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const {
    fundSearch,
    fundSort,
    goalSearch,
    goalSort,
    accountSearch,
    accountSort,
    transactionSearch,
    transactionSort,
  } = await searchParams;

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
          Search: fundSearch ?? "",
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
          Search: goalSearch ?? "",
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
          Search: accountSearch ?? "",
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
          Search: transactionSearch ?? "",
          Sort: transactionSort ?? null,
        },
      },
    },
  );
  const [
    { data, error },
    { data: fundData, error: fundError },
    { data: goalData, error: goalError },
    { data: accountData, error: accountError },
    { data: transactionData, error: transactionError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    goalPromise,
    accountPromise,
    transactionPromise,
  ]);
  if (
    typeof data === "undefined" ||
    typeof fundData === "undefined" ||
    typeof goalData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof transactionData === "undefined"
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
        <Breadcrumbs breadcrumbs={breadcrumbs.detail(data)} />
        <Stack direction="row" spacing={1}>
          {data.isOpen ? (
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
        <CaptionedValue caption="Name" value={data.name} />
        <CaptionedValue caption="Is Open" value={data.isOpen ? "Yes" : "No"} />
        <CaptionedValue
          caption="Opening Balance"
          value={formatCurrency(data.openingBalance)}
        />
        <CaptionedValue
          caption="Closing Balance"
          value={formatCurrency(data.closingBalance)}
        />
      </CaptionedFrame>
      <AccountingPeriodViewListFrames
        accountingPeriod={data}
        fundData={fundData.items}
        fundTotalCount={fundData.totalCount}
        goalData={goalData.items}
        goalTotalCount={goalData.totalCount}
        accountData={accountData.items}
        accountTotalCount={accountData.totalCount}
        transactionData={transactionData.items}
        transactionTotalCount={transactionData.totalCount}
      />
    </Stack>
  );
};

export type { AccountingPeriodViewParams, AccountingPeriodViewSearchParams };
export default AccountingPeriodView;
