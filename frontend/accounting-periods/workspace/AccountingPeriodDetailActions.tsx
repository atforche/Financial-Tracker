"use client";

import { Divider, IconButton, Menu, MenuItem } from "@mui/material";
import { type JSX, useState } from "react";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import CloseAccountingPeriodForm from "@/accounting-periods/workspace/CloseAccountingPeriodForm";
import DeleteAccountingPeriodForm from "@/accounting-periods/workspace/DeleteAccountingPeriodForm";
import MoreVert from "@mui/icons-material/MoreVert";
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
 * Provides period-specific lifecycle actions.
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
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);

  if (!canWrite) {
    return null;
  }
  const open = function (next: "close" | "reopen" | "delete"): void {
    setAnchor(null);
    setDialog(next);
  };

  return (
    <>
      <IconButton
        aria-label="Accounting period actions"
        onClick={(event) => {
          setAnchor(event.currentTarget);
        }}
      >
        <MoreVert />
      </IconButton>
      <Menu
        anchorEl={anchor}
        open={anchor !== null}
        onClose={() => {
          setAnchor(null);
        }}
      >
        <MenuItem
          onClick={() => {
            open(accountingPeriod.isOpen ? "close" : "reopen");
          }}
        >
          {accountingPeriod.isOpen ? "Close period" : "Reopen period"}
        </MenuItem>
        {accountingPeriod.isOpen ? <Divider /> : null}
        {accountingPeriod.isOpen ? (
          <MenuItem
            sx={{ color: "error.main" }}
            onClick={() => {
              open("delete");
            }}
          >
            Delete period
          </MenuItem>
        ) : null}
      </Menu>
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
