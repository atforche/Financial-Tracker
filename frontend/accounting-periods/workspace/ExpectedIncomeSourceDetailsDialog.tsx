"use client";

import { Button, Stack, Typography } from "@mui/material";
import Dialog from "@/framework/dialog/Dialog";
import type { ExpectedIncomeSource } from "@/accounting-periods/types";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";

/** 
 * Props for the ExpectedIncomeSourceDetailsDialog component.
 */
interface ExpectedIncomeSourceDetailsDialogProps {
  readonly source: ExpectedIncomeSource;
  readonly canManage: boolean;
  readonly onClose: () => void;
  readonly onChange: () => void;
  readonly onDelete: () => void;
}

/**
 * Displays an expected-income source and offers its available actions.
 */
const ExpectedIncomeSourceDetailsDialog = function ({
  source,
  canManage,
  onClose,
  onChange,
  onDelete,
}: ExpectedIncomeSourceDetailsDialogProps): JSX.Element {
  return (
    <Dialog
      open
      onClose={onClose}
      fullWidth
      maxWidth="sm"
      title={source.name}
      actions={
        <>
          <Button onClick={onClose}>Close</Button>
          {canManage ? (
            <>
              <Button onClick={onChange}>Change</Button>
              <Button color="error" onClick={onDelete}>
                Delete
              </Button>
            </>
          ) : null}
        </>
      }
    >
      <Stack spacing={1.5}>
        <Typography>
          Expected income: {formatCurrency(source.expectedAmount)}
        </Typography>
        <Typography>Per payment: {formatCurrency(source.netAmount)}</Typography>
        <Typography>Expected payments: {source.expectedDates.length}</Typography>
        <Typography color="text.secondary">
          Payment dates: {source.expectedDates.join(", ") || "None"}
        </Typography>
      </Stack>
    </Dialog>
  );
};

export default ExpectedIncomeSourceDetailsDialog;
