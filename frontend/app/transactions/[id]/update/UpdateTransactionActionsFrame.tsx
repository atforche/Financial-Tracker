"use client";

import { Button, DialogActions } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { FundAmount } from "@/data/fundTypes";
import Link from "next/link";
import type { Transaction } from "@/data/transactionTypes";
import updateTransaction from "@/app/transactions/[id]/update/updateTransaction";

/**
 * Props for the UpdateTransactionActionsFrame component.
 */
interface UpdateTransactionActionsFrameProps {
  readonly redirectUrl: string;
  readonly transaction: Transaction;
  readonly date: Dayjs | null;
  readonly location: string;
  readonly description: string;
  readonly debitFundAmounts: FundAmount[];
  readonly creditFundAmounts: FundAmount[];
}

/**
 * Component that displays the action buttons for updating a transaction.
 */
const UpdateTransactionActionsFrame = function ({
  redirectUrl,
  transaction,
  date,
  location,
  description,
  debitFundAmounts,
  creditFundAmounts,
}: UpdateTransactionActionsFrameProps): JSX.Element {
  const [state, action, pending] = useActionState(updateTransaction, {
    transactionId: transaction.id,
    redirectUrl,
  });

  const trimmedLocation = location.trim();
  const trimmedDescription = description.trim();

  const canBeSubmitted = function (): boolean {
    if (date === null) {
      return false;
    }
    if (trimmedLocation === "") {
      return false;
    }
    if (trimmedDescription === "") {
      return false;
    }
    if (
      transaction.debitAccount !== null &&
      typeof transaction.debitAccount !== "undefined" &&
      debitFundAmounts.filter(
        (fundAmount) => fundAmount.fundId !== "" && fundAmount.amount > 0,
      ).length === 0
    ) {
      return false;
    }
    if (
      transaction.creditAccount !== null &&
      typeof transaction.creditAccount !== "undefined" &&
      creditFundAmounts.filter(
        (fundAmount) => fundAmount.fundId !== "" && fundAmount.amount > 0,
      ).length === 0
    ) {
      return false;
    }
    return true;
  };

  return (
    <>
      <DialogActions sx={{ maxWidth: "800px" }}>
        <Link href={redirectUrl} tabIndex={-1}>
          <Button variant="outlined">Cancel</Button>
        </Link>
        <Button
          variant="contained"
          loading={pending}
          disabled={!canBeSubmitted()}
          onClick={() => {
            if (date === null || !canBeSubmitted()) {
              return;
            }
            startTransition(() => {
              action({
                date: date.format("YYYY-MM-DD"),
                location: trimmedLocation,
                description: trimmedDescription,
                debitAccount:
                  transaction.debitAccount === null ||
                  typeof transaction.debitAccount === "undefined"
                    ? null
                    : {
                        fundAmounts: debitFundAmounts,
                      },
                creditAccount:
                  transaction.creditAccount === null ||
                  typeof transaction.creditAccount === "undefined"
                    ? null
                    : {
                        fundAmounts: creditFundAmounts,
                      },
              });
            });
          }}
        >
          Update
        </Button>
      </DialogActions>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={
          [state.dateErrors, state.unmappedErrors].filter(Boolean).join(" ") ||
          null
        }
      />
    </>
  );
};

export default UpdateTransactionActionsFrame;
