import {
  type PaperProps,
  type SxProps,
  type Theme,
  alpha,
} from "@mui/material";
import type { SystemStyleObject } from "@mui/system";

/**
 * Represents a resolved style object or function for the dialog component.
 */
type ResolvedSx =
  | boolean
  | SystemStyleObject<Theme>
  | ((theme: Theme) => SystemStyleObject<Theme>);

/**
 * Determines whether the given sx prop is an array of resolved styles.
 */
const isResolvedSxArray = function (
  sx: SxProps<Theme>,
): sx is readonly ResolvedSx[] {
  return Array.isArray(sx);
};

/**
 * Appends the given sx prop to the list of resolved styles.
 */
const appendSx = function (
  items: ResolvedSx[],
  sx: SxProps<Theme> | null,
): void {
  if (sx === null || typeof sx === "undefined") {
    return;
  }
  if (isResolvedSxArray(sx)) {
    sx.forEach((entry) => {
      items.push(entry);
    });
    return;
  }
  items.push(sx);
};

/**
 * Builds the sx prop for the dialog's paper component.
 */
const buildPaperSx = function (
  baseSx: PaperProps["sx"] | null,
): readonly ResolvedSx[] {
  const sharedSx = (theme: Theme): SystemStyleObject<Theme> => ({
    overflow: "hidden",
    borderRadius: 5,
    border: `1px solid ${alpha(theme.palette.divider, 0.72)}`,
    backgroundColor: theme.palette.background.paper,
  });
  const sxItems: ResolvedSx[] = [sharedSx];

  appendSx(sxItems, baseSx ?? null);

  return sxItems;
};

export type { ResolvedSx };
export { appendSx, buildPaperSx };
