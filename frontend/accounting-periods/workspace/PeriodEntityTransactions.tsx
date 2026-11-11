"use client";

import { Stack, Typography } from "@mui/material";
import getTransactionRow, {
  type EntityEvent,
} from "@/balance-events/getTransactionRow";
import ActionIconButton from "@/framework/view/ActionIconButton";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import RowList from "@/framework/view/RowList";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the PeriodEntityTransactions component.
 */
interface PeriodEntityTransactionsProps {
  readonly entityName: "account" | "fund";
  readonly events: readonly EntityEvent[];
  readonly addTransactionHref: string;
  readonly isOpen: boolean;
  readonly returnUrl: string;
}

/**
 * Lists how transactions affected an account or fund in an accounting period.
 */
const PeriodEntityTransactions = function ({
  entityName,
  events,
  addTransactionHref,
  isOpen,
  returnUrl,
}: PeriodEntityTransactionsProps): JSX.Element {
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
          {canWrite && isOpen ? (
            <ActionIconButton
              action="add"
              label="Add transaction"
              href={addTransactionHref}
            />
          ) : null}
        </Stack>
        {events.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No transactions affected this {entityName} in this period.
          </Typography>
        ) : (
          <RowList
            rows={events.map((event, index) => ({
              ...getTransactionRow(event, index, returnUrl),
              ...(event.isPosted ? {} : { tag: "Pending" }),
            }))}
          />
        )}
      </Stack>
    </ContentSurface>
  );
};

export default PeriodEntityTransactions;
