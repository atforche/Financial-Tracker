import { type SxProps, type Theme, alpha } from "@mui/material";
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
 * Determines whether the given sx prop is an array of resolved styles.
 */
const toSxArray = function (sx?: SxProps<Theme>): readonly ResolvedSx[] {
  if (typeof sx === "undefined") {
    return [];
  }
  return isResolvedSxArray(sx) ? sx : [sx];
};

/**
 * Builds the sx prop for the dialog's paper component.
 */
const buildPaperSx = function (baseSx?: SxProps<Theme>): readonly ResolvedSx[] {
  const sharedSx = (theme: Theme): SystemStyleObject<Theme> => ({
    overflow: "hidden",
    borderRadius: 5,
    border: `1px solid ${alpha(theme.palette.divider, 0.72)}`,
    backgroundColor: "background.paper",
  });
  return [sharedSx, ...toSxArray(baseSx)];
};

export type { ResolvedSx };
export { buildPaperSx, toSxArray };
