"use client";

import { Button, type ButtonProps, Stack, Typography } from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";

/**
 * Props for the ConfirmActionDialog component.
 */
interface ConfirmActionDialogProps {
  readonly trigger: (openDialog: () => void) => JSX.Element;
  readonly title: string;
  readonly confirmationCopy: ReactNode;
  readonly confirmLabel: string;
  readonly onConfirm: () => void;
  readonly pending: boolean;
  readonly errorTitle?: string | null | undefined;
  readonly unmappedErrors?: string | null | undefined;
  readonly confirmColor?: ButtonProps["color"];
}

/**
 * Displays an action trigger and the matching confirmation dialog.
 */
const ConfirmActionDialog = function ({
  trigger,
  title,
  confirmationCopy,
  confirmLabel,
  onConfirm,
  pending,
  errorTitle = null,
  unmappedErrors = null,
  confirmColor = "primary",
}: ConfirmActionDialogProps): JSX.Element {
  const [open, setOpen] = useState(false);

  const closeDialog = function (): void {
    setOpen(false);
  };

  return (
    <>
      {trigger(() => {
        setOpen(true);
      })}
      <Dialog
        open={open}
        onClose={pending ? undefined : closeDialog}
        fullWidth
        maxWidth="sm"
        title={title}
        actions={
          <>
            <Button disabled={pending} onClick={closeDialog}>
              Cancel
            </Button>
            <Button
              color={confirmColor}
              variant="contained"
              loading={pending}
              onClick={onConfirm}
            >
              {confirmLabel}
            </Button>
          </>
        }
      >
        <Stack spacing={2}>
          <Typography>{confirmationCopy}</Typography>
          <ErrorAlert
            errorMessage={errorTitle}
            unmappedErrors={unmappedErrors}
          />
        </Stack>
      </Dialog>
    </>
  );
};

export type { ConfirmActionDialogProps };
export default ConfirmActionDialog;
