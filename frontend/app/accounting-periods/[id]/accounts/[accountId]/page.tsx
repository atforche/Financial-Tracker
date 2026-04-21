import { Button, Stack } from "@mui/material";
import routes, { routeBreadcrumbs, withQuery } from "@/framework/routes";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/data/getApiClient";
import { isPositiveChangeInBalance } from "@/data/accountTypes";

/**
 * Props for the Page component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
    accountId: string;
  }>;
}

const formatAmount = function (value: number | null): string {
  return value === null ? "-" : formatCurrency(value);
};

/**
 * Component that displays the view for a single account in the context of an accounting period.
 */
const Page = async function ({ params }: PageProps): Promise<JSX.Element> {
  const { id, accountId } = await params;

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
  const accountPromise = apiClient.GET(
    "/accounts/{accountId}/{accountingPeriodId}",
    {
      params: {
        path: {
          accountId,
          accountingPeriodId: id,
        },
      },
    },
  );

  const [
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: accountData, error: accountError },
  ] = await Promise.all([accountingPeriodPromise, accountPromise]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof accountData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch account with ID ${accountId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? accountError?.detail ?? "Unknown error"}`,
    );
  }

  const changeInPostedBalance =
    accountData.closingBalance.postedBalance -
    accountData.openingBalance.postedBalance;
  const isPositiveChange = isPositiveChangeInBalance(
    accountData.type,
    changeInPostedBalance,
  );

  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        maxWidth={1000}
      >
        <Breadcrumbs
          breadcrumbs={routeBreadcrumbs.accountingPeriods.accountDetail(
            {
              id,
              name: accountingPeriodData.name,
            },
            {
              id: accountId,
              name: accountData.name,
            },
          )}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={withQuery(routes.accounts.update(accountId), {
              accountingPeriodId: id,
            })}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={withQuery(routes.accounts.delete(accountId), {
              accountingPeriodId: id,
            })}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={accountData.name} />
        <CaptionedValue caption="Type" value={accountData.type} />
      </CaptionedFrame>
      <CaptionedFrame caption="Opening Balance">
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(accountData.openingBalance.postedBalance)}
        />
        <CaptionedValue
          caption="Available to Spend"
          value={formatAmount(accountData.openingBalance.availableToSpend)}
        />
        <CaptionedValue
          caption="Pending Debit"
          value={formatCurrency(accountData.openingBalance.pendingDebitAmount)}
        />
        <CaptionedValue
          caption="Pending Credit"
          value={formatCurrency(accountData.openingBalance.pendingCreditAmount)}
        />
      </CaptionedFrame>
      <CaptionedFrame caption="Closing Balance">
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(accountData.closingBalance.postedBalance)}
        />
        <CaptionedValue
          caption="Available to Spend"
          value={formatAmount(accountData.closingBalance.availableToSpend)}
        />
        <CaptionedValue
          caption="Pending Debit"
          value={formatCurrency(accountData.closingBalance.pendingDebitAmount)}
        />
        <CaptionedValue
          caption="Pending Credit"
          value={formatCurrency(accountData.closingBalance.pendingCreditAmount)}
        />
        <CaptionedValue
          caption="Change in Posted Balance"
          value={
            <span style={{ color: isPositiveChange ? "green" : "red" }}>
              {changeInPostedBalance >= 0 ? "+" : "-"}{" "}
              {formatCurrency(Math.abs(changeInPostedBalance))}
            </span>
          }
        />
      </CaptionedFrame>
    </Stack>
  );
};

export default Page;
