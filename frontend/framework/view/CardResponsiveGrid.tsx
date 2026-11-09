import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";

/**
 * Props for the CardResponsiveGrid component.
 */
interface CardResponsiveGridProps {
  readonly children: ReactNode;
  readonly minimumColumnWidth: number;
  readonly spacing?: number;
}

/**
 * Arranges cards in content-sized columns that collapse on narrow screens.
 */
const CardResponsiveGrid = function ({
  children,
  minimumColumnWidth,
  spacing = 3,
}: CardResponsiveGridProps): JSX.Element {
  return (
    <Box
      sx={{
        display: "grid",
        gap: spacing,
        justifyContent: "start",
        justifyItems: "stretch",
        alignItems: "start",
        gridTemplateColumns: {
          xs: "minmax(0, 1fr)",
          sm: `repeat(auto-fit, minmax(${minimumColumnWidth}px, max-content))`,
        },
        "& .MuiTypography-root": {
          whiteSpace: { xs: "normal", sm: "nowrap" },
        },
      }}
    >
      {children}
    </Box>
  );
};

export default CardResponsiveGrid;
