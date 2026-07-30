import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";
import type { Breakpoint } from "@mui/material/styles";

/**
 * Defines the number of columns for each breakpoint in a responsive grid layout.
 */
type ResponsiveGridColumns = Partial<Record<Breakpoint, number>>;

/**
 * Props for the FixedColumnResponsiveGrid component.
 */
interface FixedColumnResponsiveGridProps {
  readonly columns: ResponsiveGridColumns;
  readonly minimumColumnWidth?: never;
}

/**
 * Props for the AutoFitResponsiveGrid component.
 */
interface AutoFitResponsiveGridProps {
  readonly columns?: never;
  readonly minimumColumnWidth: number;
}

/**
 * Props for the ResponsiveGrid component.
 */
type ResponsiveGridProps = {
  readonly children: ReactNode;
  readonly spacing?: number;
} & (FixedColumnResponsiveGridProps | AutoFitResponsiveGridProps);

/**
 * Arranges page-level content in either fixed responsive columns or auto-fit columns.
 */
const ResponsiveGrid = function ({
  children,
  columns,
  minimumColumnWidth,
  spacing = 3,
}: ResponsiveGridProps): JSX.Element {
  const gridTemplateColumns =
    typeof minimumColumnWidth === "number"
      ? `repeat(auto-fit, minmax(min(100%, ${minimumColumnWidth}px), 1fr))`
      : Object.fromEntries(
          Object.entries(columns).map(([breakpoint, count]) => [
            breakpoint,
            `repeat(${count}, minmax(0, 1fr))`,
          ]),
        );

  return (
    <Box sx={{ display: "grid", gap: spacing, gridTemplateColumns }}>
      {children}
    </Box>
  );
};

export default ResponsiveGrid;
export type { ResponsiveGridColumns };
