import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";

/**
 * Props for the CardResponsiveGrid component.
 */
interface CardResponsiveGridProps {
  readonly children: ReactNode;
  readonly columns?: number;
  readonly contentSized?: boolean;
  readonly minimumColumnWidth: number;
  readonly spacing?: number;
}

/**
 * Arranges cards in content-sized columns that collapse on narrow screens.
 */
const CardResponsiveGrid = function ({
  children,
  columns,
  contentSized = false,
  minimumColumnWidth,
  spacing = 3,
}: CardResponsiveGridProps): JSX.Element {
  return (
    <Box
      sx={{
        display: "grid",
        gap: spacing,
        width: contentSized ? { xs: "100%", sm: "max-content" } : undefined,
        justifyContent: "start",
        justifyItems: "stretch",
        alignItems: "start",
        gridTemplateColumns: {
          xs: "minmax(0, 1fr)",
          sm:
            typeof columns === "number"
              ? `repeat(${columns}, minmax(${minimumColumnWidth}px, max-content))`
              : `repeat(auto-fit, minmax(${minimumColumnWidth}px, max-content))`,
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
