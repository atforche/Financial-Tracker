import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";

/**
 * Props for the SummaryCardGrid component.
 */
interface SummaryCardGridProps {
  readonly children: ReactNode;
}

/**
 * Lays out a responsive group of summary cards.
 */
const SummaryCardGrid = function ({
  children,
}: SummaryCardGridProps): JSX.Element {
  return (
    <Box
      sx={{
        display: "grid",
        gap: 2,
        gridTemplateColumns: {
          xs: "1fr",
          md: "repeat(2, minmax(0, 1fr))",
          xl: "repeat(3, minmax(0, 1fr))",
        },
      }}
    >
      {children}
    </Box>
  );
};

export default SummaryCardGrid;
