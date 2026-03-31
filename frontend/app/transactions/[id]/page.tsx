import { Button, Stack } from "@mui/material";
import type { Account } from "@/data/accountTypes";
import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { Fund } from "@/data/fundTypes";
import type { JSX } from "react";
import type { Transaction } from "@/data/transactionTypes";
import TransactionAccountFrame from "@/app/transactions/[id]/TransactionAccountFrame";
import TransactionFundBalanceFrame from "@/app/transactions/[id]/TransactionFundBalanceFrame";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    accountingPeriodId?: string;
    accountId?: string;
    fundId?: string;
  }>;
}

/**
 * Gets the breadcrumbs to be displayed at the top of the page.
 */
const getBreadcrumbs = function (
  transaction: Transaction,
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): JSX.Element {
  if (accountingPeriod !== null && account !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: account.name,
            href: `/accounting-periods/${accountingPeriod.id}/accounts/${account.id}`,
          },
          {
            label: `Transaction`,
            href: `/transactions/${transaction.id}`,
          },
        ]}
      />
    );
  }
  if (accountingPeriod !== null && fund !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: fund.name,
            href: `/accounting-periods/${accountingPeriod.id}/funds/${fund.id}`,
          },
          {
            label: `Transaction`,
            href: `/transactions/${transaction.id}`,
          },
        ]}
      />
    );
  }
  if (accountingPeriod !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounting Periods",
            href: "/accounting-periods",
          },
          {
            label: accountingPeriod.name,
            href: `/accounting-periods/${accountingPeriod.id}`,
          },
          {
            label: `Transaction`,
            href: `/transactions/${transaction.id}`,
          },
        ]}
      />
    );
  }
  if (account !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Accounts",
            href: "/accounts",
          },
          {
            label: account.name,
            href: `/accounts/${account.id}`,
          },
          {
            label: `Transaction`,
            href: `/transactions/${transaction.id}`,
          },
        ]}
      />
    );
  }
  if (fund !== null) {
    return (
      <Breadcrumbs
        breadcrumbs={[
          {
            label: "Funds",
            href: "/funds",
          },
          {
            label: fund.name,
            href: `/funds/${fund.id}`,
          },
          {
            label: `Transaction`,
            href: `/transactions/${transaction.id}`,
          },
        ]}
      />
    );
  }
  throw new Error(
    "Invalid state: Transaction details page must have either an associated accounting period, account, or fund",
  );
};

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
        {getBreadcrumbs(
          transactionData,
          accountingPeriodData,
          accountData,
          fundData,
        )}
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
