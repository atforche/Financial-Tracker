"use client";

import {
  type AccountingPeriod,
  getDefaultDate,
} from "@/data/accountingPeriodTypes";
import { Button, DialogActions } from "@mui/material";
import { type JSX, startTransition, useActionState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountIdentifier } from "@/data/accountTypes";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import type { FundAmount } from "@/data/fundTypes";
import Link from "next/link";
import ToggleState from "@/app/transactions/create/toggleState";
import createTransaction from "@/app/transactions/create/createTransaction";

/**
 * Props for the CreateTransactionActionsFrame component.
 */
interface CreateTransactionActionsFrameProps {
  readonly redirectUrl: string;
  readonly toggleState: ToggleState;
  readonly accountingPeriod: AccountingPeriod | null;
  readonly date: Dayjs | null;
  readonly location: string;
  readonly description: string;
  readonly debitAccount: AccountIdentifier | null;
  readonly debitFundAmounts: FundAmount[];
  readonly creditAccount: AccountIdentifier | null;
  readonly creditFundAmounts: FundAmount[];
}

/**
 * Component that displays the action buttons for creating a transaction.
 */
const CreateTransactionActionsFrame = function ({
  redirectUrl,
  toggleState,
  accountingPeriod,
  date,
  location,
  description,
  debitAccount,
  debitFundAmounts,
  creditAccount,
  creditFundAmounts,
}: CreateTransactionActionsFrameProps): JSX.Element {
  const [state, action, pending] = useActionState(createTransaction, {
    redirectUrl,
  });

  const defaultDate =
    accountingPeriod === null ? null : getDefaultDate(accountingPeriod);
  const trimmedLocation = location.trim();
  const trimmedDescription = description.trim();

  const isDebitAccountValid = function (): boolean {
    return (
      debitAccount !== null &&
      debitFundAmounts.filter(
        (fundAmount) => fundAmount.fundId !== "" && fundAmount.amount > 0,
      ).length > 0
    );
  };

  const isCreditAccountValid = function (): boolean {
    return (
      creditAccount !== null &&
      creditFundAmounts.filter(
        (fundAmount) => fundAmount.fundId !== "" && fundAmount.amount > 0,
      ).length > 0
    );
  };

  const canBeSubmitted = function (): boolean {
    if (accountingPeriod === null) {
      return false;
    }
    if (date === null && defaultDate === null) {
      return false;
    }
    if (trimmedLocation === "") {
      return false;
    }
    if (trimmedDescription === "") {
      return false;
    }
    if (toggleState === ToggleState.Debit) {
      return isDebitAccountValid();
    } else if (toggleState === ToggleState.Credit) {
      return isCreditAccountValid();
    }
    return isDebitAccountValid() && isCreditAccountValid();
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
            if (accountingPeriod === null || !canBeSubmitted()) {
              return;
            }
            startTransition(() => {
              action({
                accountingPeriodId: accountingPeriod.id,
                date:
                  date?.format("YYYY-MM-DD") ??
                  defaultDate?.format("YYYY-MM-DD") ??
                  dayjs().format("YYYY-MM-DD"),
                location: trimmedLocation,
                description: trimmedDescription,
                debitAccount:
                  debitAccount === null
                    ? null
                    : {
                        accountId: debitAccount.id,
                        fundAmounts: debitFundAmounts,
                      },
                creditAccount:
                  creditAccount === null
                    ? null
                    : {
                        accountId: creditAccount.id,
                        fundAmounts: creditFundAmounts,
                      },
              });
            });
          }}
        >
          Create
        </Button>
      </DialogActions>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
    </>
  );
};

export default CreateTransactionActionsFrame;
