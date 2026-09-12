"use client";

import { Button, Stack } from "@mui/material";
import { type JSX, useState } from "react";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import CloseAccountingPeriodForm from "@/accounting-periods/workspace/CloseAccountingPeriodForm";
import DeleteAccountingPeriodForm from "@/accounting-periods/workspace/DeleteAccountingPeriodForm";
import Link from "next/link";
import ReopenAccountingPeriodForm from "@/accounting-periods/workspace/ReopenAccountingPeriodForm";
import type { Route } from "next";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountingPeriodDetailActions component.
 */
interface AccountingPeriodDetailActionsProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly planHref: Route;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
}

/**
 * Provides period-specific lifecycle and income setup actions.
 */
const AccountingPeriodDetailActions = function ({
  accountingPeriod,
  planHref,
  redirectUrl,
  deleteRedirectUrl,
}: AccountingPeriodDetailActionsProps): JSX.Element {
  const canWrite = useWriteAccess();
  const [dialog, setDialog] = useState<"close" | "reopen" | "delete" | null>(
    null,
  );

  return (
    <>
      <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
        <Button component={Link} href={planHref} variant="outlined">
          {accountingPeriod.isOpen ? "Edit Plan" : "View Plan"}
        </Button>
        {canWrite ? (
          <>
            <Button
              variant="contained"
              onClick={() => {
                setDialog(accountingPeriod.isOpen ? "close" : "reopen");
              }}
            >
              {accountingPeriod.isOpen ? "Close" : "Reopen"}
            </Button>
            {accountingPeriod.isOpen ? (
              <Button
                color="error"
                variant="outlined"
                onClick={() => {
                  setDialog("delete");
                }}
              >
                Delete
              </Button>
            ) : null}
          </>
        ) : null}
      </Stack>
      {dialog === "close" ? (
        <CloseAccountingPeriodForm
          accountingPeriod={accountingPeriod}
          onClose={() => {
            setDialog(null);
          }}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {dialog === "reopen" ? (
        <ReopenAccountingPeriodForm
          accountingPeriod={accountingPeriod}
          onClose={() => {
            setDialog(null);
          }}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {dialog === "delete" ? (
        <DeleteAccountingPeriodForm
          accountingPeriod={accountingPeriod}
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
