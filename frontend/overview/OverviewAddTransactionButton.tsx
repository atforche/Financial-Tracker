"use client";

import AddIcon from "@mui/icons-material/Add";
import { Button } from "@mui/material";
import type { JSX } from "react";
import Link from "next/link";
import transactionRoutes from "@/transactions/routes";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Links to transaction entry, returning to the overview, for users who can make changes.
 */
const OverviewAddTransactionButton = function (): JSX.Element | null {
  const canWrite = useWriteAccess();
  return canWrite ? (
    <Button
      component={Link}
      href={transactionRoutes.workspaceCreate({ returnUrl: "/" })}
      variant="contained"
      startIcon={<AddIcon />}
    >
      Add Transaction
    </Button>
  ) : null;
};

export default OverviewAddTransactionButton;
