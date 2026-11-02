"use client";

import { usePathname, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/helpers";
import { Button } from "@mui/material";
import CreateAccountingPeriodForm from "@/accounting-periods/workspace/CreateAccountingPeriodForm";
import type { JSX } from "react";
import { buildUrl } from "@/framework/routes/helpers";
import { getNextAccountingPeriod } from "@/accounting-periods/helpers";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountingPeriodWorkspaceActions component.
 */
interface AccountingPeriodWorkspaceActionsProps {
  readonly latestAccountingPeriod: AccountingPeriod | null;
  readonly requestedAction: AccountingPeriodWorkspaceAction | null;
}

/**
 * Displays the button that creates the next accounting period, or the first when there are none, and its dialog.
 */
const AccountingPeriodWorkspaceActions = function ({
  latestAccountingPeriod,
  requestedAction,
}: AccountingPeriodWorkspaceActionsProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const setAction = function (
    action: AccountingPeriodWorkspaceAction | null,
  ): void {
    updateParams((params) => {
      if (action === null) {
        params.delete("action");
      } else {
        params.set("action", action);
      }
    });
  };
  const dialogParams = new URLSearchParams(searchParams.toString());
  dialogParams.delete("action");
  const dialogRedirectUrl = buildUrl(pathname, dialogParams);

  if (!canWrite) {
    return null;
  }

  return (
    <>
      <Button
        variant="contained"
        onClick={() => {
          setAction("create");
        }}
      >
        {latestAccountingPeriod === null
          ? "Create First Period"
          : `Create ${getNextAccountingPeriod(latestAccountingPeriod).name}`}
      </Button>
      {requestedAction === "create" ? (
        <CreateAccountingPeriodForm
          latestAccountingPeriod={latestAccountingPeriod}
          onClose={() => {
            setAction(null);
          }}
          redirectUrl={dialogRedirectUrl}
        />
      ) : null}
    </>
  );
};

export default AccountingPeriodWorkspaceActions;
