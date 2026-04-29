import type {
  AccountingPeriodFundGoalSortOrder,
  AccountingPeriodFundSortOrder,
} from "@/data/fundTypes";
import { Button, Stack } from "@mui/material";
import routes, { routeBreadcrumbs } from "@/framework/routes";
import type { AccountingPeriodAccountSortOrder } from "@/data/accountTypes";
import AccountingPeriodListFrames from "@/app/accounting-periods/[id]/AccountingPeriodListFrames";
import type { AccountingPeriodTransactionSortOrder } from "@/data/transactionTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    fundSearch?: string;
    fundSort?: AccountingPeriodFundSortOrder;
    fundGoalSearch?: string;
    fundGoalSort?: AccountingPeriodFundGoalSortOrder;
    accountSearch?: string;
    accountSort?: AccountingPeriodAccountSortOrder;
    transactionSearch?: string;
    transactionSort?: AccountingPeriodTransactionSortOrder;
  }>;
}

/**
 * Component that displays the view for a single accounting period.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const {
    fundSearch,
    fundSort,
    fundGoalSearch,
    fundGoalSort,
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
  const fundGoalPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/fund-goals",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
        query: {
          Search: fundGoalSearch ?? "",
          Sort: fundGoalSort ?? null,
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
    { data: fundGoalData, error: fundGoalError },
    { data: accountData, error: accountError },
    { data: transactionData, error: transactionError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    fundGoalPromise,
    accountPromise,
    transactionPromise,
  ]);
  if (
    typeof data === "undefined" ||
    typeof fundData === "undefined" ||
    typeof fundGoalData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof transactionData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${error?.detail ?? fundError?.detail ?? fundGoalError?.detail ?? accountError?.detail ?? transactionError?.detail}`,
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
        <Breadcrumbs
          breadcrumbs={routeBreadcrumbs.accountingPeriods.detail({
            id,
            name: data.name,
          })}
        />
        <Stack direction="row" spacing={1}>
          {data.isOpen ? (
            <Button
              variant="contained"
              color="primary"
              href={routes.accountingPeriods.close(id)}
            >
              Close
            </Button>
          ) : (
            <Button
              variant="contained"
              color="primary"
              href={routes.accountingPeriods.reopen(id)}
            >
              Reopen
            </Button>
          )}
          <Button
            variant="contained"
            color="error"
            href={routes.accountingPeriods.delete(id)}
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
      <AccountingPeriodListFrames
        accountingPeriod={data}
        fundData={fundData.items}
        fundTotalCount={fundData.totalCount}
        fundGoalData={fundGoalData.items}
        fundGoalTotalCount={fundGoalData.totalCount}
        accountData={accountData.items}
        accountTotalCount={accountData.totalCount}
        transactionData={transactionData.items}
        transactionTotalCount={transactionData.totalCount}
      />
    </Stack>
  );
};

export default Page;
