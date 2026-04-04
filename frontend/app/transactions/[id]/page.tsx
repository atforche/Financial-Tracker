import { Button, Stack } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import TransactionAccountFrame from "@/app/transactions/[id]/TransactionAccountFrame";
import TransactionFundBalanceFrame from "@/app/transactions/[id]/TransactionFundBalanceFrame";
import type TransactionSearchParams from "@/app/transactions/[id]/transactionSearchParams";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";
import getBreadcrumbs from "@/app/transactions/[id]/getBreadcrumbs";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<TransactionSearchParams>;
}

/**
 * Component that displays the view for a single transaction in an accounting period.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId, accountId, fundId } = await searchParams;

  const apiClient = getApiClient();
  const transactionPromise = apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId: id,
      },
    },
  });
  const accountingPeriodPromise =
    typeof accountingPeriodId !== "undefined"
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: {
            path: {
              accountingPeriodId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });
  const accountPromise =
    typeof accountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });
  const fundPromise =
    typeof fundId !== "undefined"
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });

  const [
    { data: transactionData, error: transactionError },
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: accountData, error: accountError },
    { data: fundData, error: fundError },
  ] = await Promise.all([
    transactionPromise,
    accountingPeriodPromise,
    accountPromise,
    fundPromise,
  ]);
  if (
    typeof transactionData === "undefined" ||
    typeof accountingPeriodData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof fundData === "undefined"
  ) {
    throw new Error(
      `Failed to load data for transaction details page: ${transactionError?.detail ?? accountingPeriodError?.detail ?? accountError?.detail ?? fundError?.detail ?? "Unknown error"}`,
    );
  }

  const updateParams = new URLSearchParams();
  if (typeof accountingPeriodId !== "undefined") {
    updateParams.set("accountingPeriodId", accountingPeriodId);
  }
  if (typeof accountId !== "undefined") {
    updateParams.set("accountId", accountId);
  }
  if (typeof fundId !== "undefined") {
    updateParams.set("fundId", fundId);
  }
  const updateQueryString = updateParams.toString();
  const updateHref = `/transactions/${id}/update${updateQueryString ? `?${updateQueryString}` : ""}`;

  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={getBreadcrumbs(
            transactionData,
            accountingPeriodData,
            accountData,
            fundData,
          )}
        />
        <Button
          variant="contained"
          color="primary"
          href={updateHref}
          disabled={
            (transactionData.debitAccount === null ||
              transactionData.debitAccount?.postedDate !== null) &&
            (transactionData.creditAccount === null ||
              transactionData.creditAccount?.postedDate !== null)
          }
        >
          Edit
        </Button>
      </Stack>
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
            transaction={transactionData}
            transactionAccount={transactionData.debitAccount}
            searchParams={await searchParams}
          />
        ) : null}
        {transactionData.creditAccount ? (
          <TransactionAccountFrame
            transaction={transactionData}
            transactionAccount={transactionData.creditAccount}
            searchParams={await searchParams}
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
