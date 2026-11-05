"use client";

import { Button, Stack, Typography } from "@mui/material";
import { BalanceEventType } from "@/balance-events/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { FundGoalBalanceEvent } from "@/fund-goals/types";
import type { JSX } from "react";
import Link from "next/link";
import RowList from "@/framework/view/RowList";
import { formatBalanceEventCounterparty } from "@/balance-events/helpers";
import { formatShortDate } from "@/framework/dateHelpers";
import { formatSignedCurrency } from "@/framework/currencyHelpers";
import routes from "@/transactions/routes";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the PeriodFundTransactions component.
 */
interface PeriodFundTransactionsProps {
  readonly events: readonly FundGoalBalanceEvent[];
  readonly addTransactionHref: string;
  readonly returnUrl: string;
}

/**
 * Lists how transactions affected a fund in an accounting period.
 */
const PeriodFundTransactions = function ({
  events,
  addTransactionHref,
  returnUrl,
}: PeriodFundTransactionsProps): JSX.Element {
  const canWrite = useWriteAccess();
  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Stack
          direction="row"
          spacing={2}
          sx={{ alignItems: "center", justifyContent: "space-between" }}
        >
          <Typography variant="h5">Transactions</Typography>
          {canWrite ? (
            <Button
              component={Link}
              href={addTransactionHref}
              variant="contained"
            >
              Add Transaction
            </Button>
          ) : null}
        </Stack>
        {events.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No transactions affected this fund in this period.
          </Typography>
        ) : (
          <RowList
            rows={events.map((event) => {
              const isCredit = event.type === BalanceEventType.Credit;
              return {
                key: `${event.transactionId}-${event.type}-${event.amount.toString()}-${event.eventDate ?? ""}`,
                href: routes.workspaceDetail(event.transactionId, {
                  returnUrl,
                }),
                leading: formatShortDate(
                  new Date(
                    `${event.eventDate ?? event.transactionDate}T00:00:00`,
                  ),
                ),
                title: event.description,
                detail: formatBalanceEventCounterparty(event),
                value: formatSignedCurrency(
                  isCredit ? event.amount : -event.amount,
                ),
                ...(isCredit ? { valueColor: "success.main" } : {}),
                ...(event.isPosted ? {} : { tag: "Pending" }),
              };
            })}
          />
        )}
      </Stack>
    </ContentSurface>
  );
};

export default PeriodFundTransactions;
