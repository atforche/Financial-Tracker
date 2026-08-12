"use client";

import { Button, Stack } from "@mui/material";
import { type JSX, useState } from "react";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import CloseAccountingPeriodForm from "@/accounting-periods/workspace/CloseAccountingPeriodForm";
import DeleteAccountingPeriodForm from "@/accounting-periods/workspace/DeleteAccountingPeriodForm";
import ReopenAccountingPeriodForm from "@/accounting-periods/workspace/ReopenAccountingPeriodForm";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountingPeriodDetailActions component.
 */
interface AccountingPeriodDetailActionsProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
}

/** 
 * Provides period-specific lifecycle and income setup actions.
 */
const AccountingPeriodDetailActions = function ({
  accountingPeriod,
  redirectUrl,
  deleteRedirectUrl,
}: AccountingPeriodDetailActionsProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const [dialog, setDialog] = useState<"close" | "reopen" | "delete" | null>(
    null,
  );

  if (!canWrite) {
    return null;
  }

  return (
    <>
      <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
        <Button
          variant="contained"
          onClick={() => {
            setDialog(accountingPeriod.isOpen ? "close" : "reopen");
          }}
        >
          {accountingPeriod.isOpen ? "Close Period" : "Reopen Period"}
        </Button>
        <Button
          color="error"
          variant="outlined"
          onClick={() => {
            setDialog("delete");
          }}
        >
          Delete Period
        </Button>
      </Stack>
      {dialog === "close" ? (
        <CloseAccountingPeriodForm
          accountingPeriod={accountingPeriod}
          open
          onClose={() => {
            setDialog(null);
          }}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {dialog === "reopen" ? (
        <ReopenAccountingPeriodForm
          accountingPeriod={accountingPeriod}
          open
          onClose={() => {
            setDialog(null);
          }}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {dialog === "delete" ? (
        <DeleteAccountingPeriodForm
          accountingPeriod={accountingPeriod}
          open
          onClose={() => {
            setDialog(null);
          }}
          redirectUrl={deleteRedirectUrl}
        />
      ) : null}
    </>
  );
};

export default AccountingPeriodDetailActions;
