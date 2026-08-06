"use client";

import { Button, type ButtonProps, Stack, Typography } from "@mui/material";
import { type JSX, type ReactNode, useEffect, useState } from "react";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";

/**
 * Props for the ConfirmActionDialog component.
 */
interface ConfirmActionDialogProps {
  readonly trigger: (openDialog: () => void) => ReactNode;
  readonly title: string;
  readonly confirmationCopy: ReactNode;
  readonly confirmLabel: string;
  readonly onConfirm: () => void;
  readonly pending: boolean;
  readonly success?: boolean | undefined;
  readonly errorTitle?: string | null | undefined;
  readonly unmappedErrors?: string | null | undefined;
  readonly confirmButtonProps?: Omit<ButtonProps, "children" | "onClick">;
  readonly cancelButtonProps?: Omit<ButtonProps, "children" | "onClick">;
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
  success = false,
  errorTitle = null,
  unmappedErrors = null,
  confirmButtonProps,
  cancelButtonProps,
}: ConfirmActionDialogProps): JSX.Element {
  const [open, setOpen] = useState(false);
  const [confirmationAttempted, setConfirmationAttempted] = useState(false);

  const closeDialog = function (): void {
    setOpen(false);
    setConfirmationAttempted(false);
  };

  useEffect(() => {
    if (success && !pending) {
      closeDialog();
    }
  }, [pending, success]);

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
            <Button
              {...cancelButtonProps}
              disabled={pending}
              onClick={closeDialog}
            >
              Cancel
            </Button>
            <Button
              {...confirmButtonProps}
              variant="contained"
              loading={pending}
              onClick={() => {
                setConfirmationAttempted(true);
                onConfirm();
              }}
            >
              {confirmLabel}
            </Button>
          </>
        }
      >
        <Stack spacing={2}>
          <Typography component="div">{confirmationCopy}</Typography>
          <ErrorAlert
            errorMessage={confirmationAttempted ? errorTitle : null}
            unmappedErrors={confirmationAttempted ? unmappedErrors : null}
          />
        </Stack>
      </Dialog>
    </>
  );
};

export type { ConfirmActionDialogProps };
export default ConfirmActionDialog;
