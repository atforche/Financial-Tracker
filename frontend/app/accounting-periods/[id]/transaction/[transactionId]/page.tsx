import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionAccountFrame from "@/app/accounting-periods/[id]/transaction/[transactionId]/TransactionAccountFrame";
import TransactionFundBalanceFrame from "@/app/accounting-periods/[id]/transaction/[transactionId]/TransactionFundBalanceFrame";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
    transactionId: string;
  }>;
}

/**
 * Component that displays the view for a single transaction in an accounting period.
 */
const Page = async function ({ params }: PageProps): Promise<JSX.Element> {
  const { id, transactionId } = await params;

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
  const transactionPromise = apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId,
      },
    },
  });

  const [
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: transactionData, error: transactionError },
  ] = await Promise.all([accountingPeriodPromise, transactionPromise]);
  if (
    typeof accountingPeriodData === "undefined" ||
    typeof transactionData === "undefined"
  ) {
    throw new Error(
      `Failed to load data for transaction details page: ${accountingPeriodError?.detail}, ${transactionError?.detail}`,
    );
  }

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriodData.name,
            href: `/accounting-periods/${id}`,
          },
          {
            label: `Transaction`,
            href: `/accounting-periods/${id}/transaction/${transactionData.id}`,
          },
        ]}
      />
      <CaptionedFrame caption="Details">
        <CaptionedValue
          caption="Date"
          value={dayjs(transactionData.date).format("MMMM DD, YYYY")}
        />
        <CaptionedValue
          caption="Accounting Period"
          value={transactionData.accountingPeriodName}
        />
        <CaptionedValue caption="Location" value={transactionData.location} />
        <CaptionedValue
          caption="Amount"
          value={formatCurrency(transactionData.amount)}
        />
      </CaptionedFrame>
      <Stack direction="row" spacing={2}>
        {transactionData.debitAccount ? (
          <TransactionAccountFrame
            transactionAccount={transactionData.debitAccount}
          />
        ) : null}
        {transactionData.creditAccount ? (
          <TransactionAccountFrame
            transactionAccount={transactionData.creditAccount}
          />
        ) : null}
      </Stack>
      <div style={{ width: "fit-content" }}>
        <TransactionFundBalanceFrame transaction={transactionData} />
      </div>
    </Stack>
  );
};

export default Page;
