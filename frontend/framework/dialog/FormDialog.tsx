"use client";

import { Button, type ButtonProps } from "@mui/material";
import type { JSX, ReactNode } from "react";
import Dialog from "@/framework/dialog/Dialog";

/**
 * Extra props for a FormDialog's submit button, such as a color or a form to submit.
 */
type FormDialogSubmitButtonProps = Omit<
  ButtonProps,
  "children" | "onClick" | "variant" | "loading" | "disabled"
>;

/**
 * Props for the FormDialog component.
 */
interface FormDialogProps {
  readonly open: boolean;
  readonly title: ReactNode;
  readonly onClose: () => void;
  readonly pending: boolean;
  readonly submitLabel: string;
  readonly onSubmit?: () => void;
  readonly canSubmit?: boolean;
  readonly submitButtonProps?: FormDialogSubmitButtonProps | undefined;

  /**
   * Actions shown between Cancel and the submit button.
   */
  readonly extraActions?: ReactNode;
  readonly maxWidth?: "sm" | "md";
  readonly children: ReactNode;
}

/**
 * Displays a dialog with Cancel and submit actions that cannot be dismissed while its action is pending.
 */
const FormDialog = function ({
  open,
  title,
  onClose,
  pending,
  submitLabel,
  onSubmit,
  canSubmit = true,
  submitButtonProps,
  extraActions,
  maxWidth = "sm",
  children,
}: FormDialogProps): JSX.Element {
  return (
    <Dialog
      open={open}
      {...(pending ? {} : { onClose })}
      fullWidth
      maxWidth={maxWidth}
      title={title}
      actions={
        <>
          <Button variant="outlined" disabled={pending} onClick={onClose}>
            Cancel
          </Button>
          {extraActions}
          <Button
            {...submitButtonProps}
            variant="contained"
            loading={pending}
            disabled={!canSubmit}
            {...(onSubmit === undefined ? {} : { onClick: onSubmit })}
          >
            {submitLabel}
          </Button>
        </>
      }
    >
      {children}
    </Dialog>
  );
};

export type { FormDialogSubmitButtonProps };
export default FormDialog;
