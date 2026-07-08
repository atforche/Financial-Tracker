"use client";

import {
  DialogActions,
  DialogContent,
  DialogTitle,
  Dialog as MuiDialog,
  type DialogProps as MuiDialogProps,
  type SxProps,
  type Theme,
} from "@mui/material";
import { type JSX, type ReactNode, useId } from "react";
import {
  type ResolvedSx,
  appendSx,
  buildPaperSx,
} from "@/framework/dialog/helpers";

/**
 * Props for the dialog component.
 */
interface DialogProps extends Omit<MuiDialogProps, "children"> {
  readonly title?: string;
  readonly children: ReactNode;
  readonly actions?: ReactNode;
  readonly contentSx?: SxProps<Theme>;
  readonly actionsSx?: SxProps<Theme>;
}

/**
 * Displays a modal dialog with the repo's standard visual treatment.
 */
const Dialog = function ({
  title,
  children,
  actions,
  slotProps,
  contentSx,
  actionsSx,
  ...dialogProps
}: DialogProps): JSX.Element {
  const titleId = useId();
  const paperSlotProps =
    typeof slotProps?.paper === "function" ? null : (slotProps?.paper ?? null);
  const contentStyles: ResolvedSx[] = [
    {
      px: { xs: 2.5, md: 3 },
      pb: { xs: 2.5, md: 3 },
      "&&": {
        pt: { xs: 2.5, md: 3 },
      },
    },
  ];
  const actionStyles: ResolvedSx[] = [
    {
      px: { xs: 2.5, md: 3 },
      pb: { xs: 2.5, md: 3 },
      pt: 0,
    },
  ];

  appendSx(contentStyles, contentSx ?? null);
  appendSx(actionStyles, actionsSx ?? null);

  return (
    <MuiDialog
      aria-labelledby={titleId}
      slotProps={{
        ...slotProps,
        paper: {
          ...paperSlotProps,
          sx: buildPaperSx(paperSlotProps?.sx ?? null),
        },
      }}
      {...dialogProps}
    >
      <DialogTitle
        id={titleId}
        sx={(theme) => ({
          px: { xs: 2.5, md: 3 },
          py: 1.75,
          borderBottom: `1px solid ${theme.palette.divider}`,
          backgroundColor: theme.palette.primary.main,
          color: theme.palette.primary.contrastText,
        })}
      >
        {title}
      </DialogTitle>
      <DialogContent sx={contentStyles}>{children}</DialogContent>
      {actions === null || typeof actions === "undefined" ? null : (
        <DialogActions sx={actionStyles}>{actions}</DialogActions>
      )}
    </MuiDialog>
  );
};

export type { DialogProps };
export default Dialog;
