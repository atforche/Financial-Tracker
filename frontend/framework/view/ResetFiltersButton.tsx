"use client";

import { IconButton, Tooltip } from "@mui/material";
import FilterAltOff from "@mui/icons-material/FilterAltOff";
import type { JSX } from "react";

/**
 * Props for the ResetFiltersButton component.
 */
interface ResetFiltersButtonProps {
  /**
   * Whether any filter differs from the page's defaults. The button only shows when there is something to reset.
   */
  readonly active: boolean;
  readonly onClick: () => void;
}

/**
 * Clears a page's filters, shown as an icon beside the last filter so it never needs a row of its own.
 */
const ResetFiltersButton = function ({
  active,
  onClick,
}: ResetFiltersButtonProps): JSX.Element | null {
  if (!active) {
    return null;
  }
  return (
    <Tooltip title="Reset filters">
      <IconButton aria-label="Reset filters" onClick={onClick}>
        <FilterAltOff />
      </IconButton>
    </Tooltip>
  );
};

export default ResetFiltersButton;
