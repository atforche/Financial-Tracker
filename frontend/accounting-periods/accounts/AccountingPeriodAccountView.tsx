import { Button, Stack } from "@mui/material";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import accountRoutes from "@/accounts/routes";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import formatCurrency from "@/framework/formatCurrency";
import getApiClient from "@/framework/data/getApiClient";
import { isPositiveChangeInBalance } from "@/accounts/types";

/**
 * Parameters for the AccountingPeriodAccountView component.
 */
interface AccountingPeriodAccountViewParams {
  id: string;
  accountId: string;
}

/**
 * Props for the AccountingPeriodAccountView component.
 */
interface AccountingPeriodAccountViewProps {
  readonly params: Promise<AccountingPeriodAccountViewParams>;
}

/**
 * Component that displays the view for a single account in the context of an accounting period.
 */
const AccountingPeriodAccountView = async function ({
  params,
}: AccountingPeriodAccountViewProps): Promise<JSX.Element> {
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
    "/accounting-periods/{accountingPeriodId}/accounts/{accountId}",
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
    { data: accountingPeriod, error: accountingPeriodError },
    { data: account, error: accountError },
  ] = await Promise.all([accountingPeriodPromise, accountPromise]);

  if (
    typeof accountingPeriod === "undefined" ||
    typeof account === "undefined"
  ) {
    throw new Error(
      `Failed to fetch account with ID ${accountId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? accountError?.detail ?? "Unknown error"}`,
    );
  }

  const changeInPostedBalance = account.closingBalance - account.openingBalance;
  const isPositiveChange = isPositiveChangeInBalance(
    account.type,
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
          breadcrumbs={breadcrumbs.accountDetail(accountingPeriod, account)}
        />
        <Stack direction="row" spacing={1}>
          <Button
            variant="contained"
            color="primary"
            href={accountRoutes.update(
              { id: account.id },
              {
                accountingPeriodId: id,
              },
            )}
          >
            Edit
          </Button>
          <Button
            variant="contained"
            color="error"
            href={accountRoutes.delete(
              { id: account.id },
              {
                accountingPeriodId: id,
              },
            )}
          >
            Delete
          </Button>
        </Stack>
      </Stack>
      <CaptionedFrame caption="Details">
        <CaptionedValue caption="Name" value={account.name} />
        <CaptionedValue caption="Type" value={account.type} />
      </CaptionedFrame>
      <CaptionedFrame caption="Opening Balance">
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(account.openingBalance)}
        />
      </CaptionedFrame>
      <CaptionedFrame caption="Closing Balance">
        <CaptionedValue
          caption="Posted Balance"
          value={formatCurrency(account.closingBalance)}
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

export type { AccountingPeriodAccountViewParams };
export default AccountingPeriodAccountView;
