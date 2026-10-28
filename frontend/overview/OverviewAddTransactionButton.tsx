"use client";

import { Box, Button, Fab } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import type { JSX } from "react";
import Link from "next/link";
import transactionRoutes from "@/transactions/routes";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the OverviewAddTransactionButton component.
 */
interface OverviewAddTransactionButtonProps {
  readonly returnUrl: string;
}

/**
 * Links to transaction entry, returning to the overview, for users who can make changes. Small screens get a floating
 * button instead, so it does not take up room in the page header.
 */
const OverviewAddTransactionButton = function ({
  returnUrl,
}: OverviewAddTransactionButtonProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const href = transactionRoutes.workspaceCreate({ returnUrl });
  return canWrite ? (
    <>
      <Button
        component={Link}
        href={href}
        variant="contained"
        startIcon={<AddIcon />}
        sx={{ display: { xs: "none", sm: "inline-flex" } }}
      >
        Add Transaction
      </Button>
      <Fab
        component={Link}
        href={href}
        color="primary"
        aria-label="Add Transaction"
        sx={{
          display: { xs: "inline-flex", sm: "none" },
          position: "fixed",
          right: 16,
          bottom: 16,
        }}
      >
        <AddIcon />
      </Fab>
    </>
  ) : null;
};

/**
 * Reserves room at the end of the page on small screens so the floating add transaction button does not cover it.
 */
const OverviewAddTransactionButtonSpacer = function (): JSX.Element | null {
  const canWrite = useWriteAccess();
  return canWrite ? (
    <Box sx={{ display: { xs: "block", sm: "none" }, height: 56 }} />
  ) : null;
};

export { OverviewAddTransactionButtonSpacer };
export default OverviewAddTransactionButton;
