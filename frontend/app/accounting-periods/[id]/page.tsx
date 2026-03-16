import { Stack, Typography } from "@mui/material";
import AccountListFrame from "@/app/accounting-periods/[id]/AccountListFrame";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import FundListFrame from "@/app/accounting-periods/[id]/FundListFrame";
import type { JSX } from "react";
import SearchBar from "@/framework/listframe/SearchBar";
import TransactionListFrame from "@/app/accounting-periods/[id]/TransactionListFrame";
import getApiClient from "@/data/getApiClient";

/**
 * Component that displays the view for a single accounting period.
 */
const AccountingPeriodView = async function (props: {
  readonly params: Promise<{ id: string }>;
}): Promise<JSX.Element> {
  const { id } = await props.params;

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
      },
    },
  );
  const [
    { data, error },
    { data: fundData, error: fundError },
    { data: accountData, error: accountError },
    { data: transactionData, error: transactionError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    accountPromise,
    transactionPromise,
  ]);
  if (
    typeof data === "undefined" ||
    typeof fundData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof transactionData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${error?.detail ?? fundError?.detail ?? accountError?.detail ?? transactionError?.detail}`,
    );
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          { label: "Accounting Periods", href: "/accounting-periods" },
          { label: data.name, href: `/accounting-periods/${id}` },
        ]}
      />
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={data.name} />
        <CaptionedValue caption="Is Open" value={data.isOpen ? "Yes" : "No"} />
        <CaptionedValue caption="Opening Balance" value="TBD" />
        <CaptionedValue caption="Closing Balance" value="TBD" />
      </CaptionedFrame>
      <Stack direction="row" spacing={10}>
        <Stack spacing={2} sx={{ width: "100%" }}>
          <Typography variant="h6">Funds</Typography>
          <SearchBar paramName="fundSearch" />
          <FundListFrame
            data={fundData.items}
            totalCount={fundData.totalCount}
          />
        </Stack>
        <Stack spacing={2} sx={{ width: "100%" }}>
          <Typography variant="h6">Accounts</Typography>
          <SearchBar paramName="accountSearch" />
          <AccountListFrame
            data={accountData.items}
            totalCount={accountData.totalCount}
          />
        </Stack>
        <Stack spacing={2} sx={{ width: "100%" }}>
          <Typography variant="h6">Transactions</Typography>
          <SearchBar paramName="transactionSearch" />
          <TransactionListFrame
            data={transactionData.items}
            totalCount={transactionData.totalCount}
          />
        </Stack>
      </Stack>
    </Stack>
  );
};

export default AccountingPeriodView;
