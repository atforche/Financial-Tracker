"use client";

import FormDialog, {
  type FormDialogSubmitButtonProps,
} from "@/framework/dialog/FormDialog";
import { type JSX, type ReactNode, useEffect, useState } from "react";
import { Stack, Typography } from "@mui/material";
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
  readonly confirmButtonProps?: FormDialogSubmitButtonProps;
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
      <FormDialog
        open={open}
        title={title}
        onClose={closeDialog}
        pending={pending}
        submitLabel={confirmLabel}
        onSubmit={() => {
          setConfirmationAttempted(true);
          onConfirm();
        }}
        submitButtonProps={confirmButtonProps}
      >
        <Stack spacing={2}>
          <Typography component="div">{confirmationCopy}</Typography>
          <ErrorAlert
            errorMessage={confirmationAttempted ? errorTitle : null}
            unmappedErrors={confirmationAttempted ? unmappedErrors : null}
          />
        </Stack>
      </FormDialog>
    </>
  );
};

export default ConfirmActionDialog;
