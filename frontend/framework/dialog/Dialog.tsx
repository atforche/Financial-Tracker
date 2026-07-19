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
import { buildPaperSx, toSxArray } from "@/framework/dialog/helpers";

/**
 * Props for the dialog component.
 */
interface DialogProps extends Omit<MuiDialogProps, "children" | "title"> {
  readonly title: ReactNode;
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
  const paperSlotProps = slotProps?.paper;
  const contentStyles: SxProps<Theme> = [
    {
      px: { xs: 2.5, md: 3 },
      pb: { xs: 2.5, md: 3 },
      "&&": {
        pt: { xs: 2.5, md: 3 },
      },
    },
    ...toSxArray(contentSx),
  ];
  const actionStyles: SxProps<Theme> = [
    {
      px: { xs: 2.5, md: 3 },
      pb: { xs: 2.5, md: 3 },
      pt: 0,
    },
    ...toSxArray(actionsSx),
  ];
  const resolvedPaperSlotProps =
    typeof paperSlotProps === "function"
      ? (
          ownerState: Parameters<typeof paperSlotProps>[0],
        ): ReturnType<typeof paperSlotProps> => {
          const resolvedProps = paperSlotProps(ownerState);
          return {
            ...resolvedProps,
            sx: buildPaperSx(resolvedProps.sx),
          };
        }
      : {
          ...paperSlotProps,
          sx: buildPaperSx(paperSlotProps?.sx),
        };

  return (
    <MuiDialog
      aria-labelledby={titleId}
      slotProps={{
        ...slotProps,
        paper: resolvedPaperSlotProps,
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
