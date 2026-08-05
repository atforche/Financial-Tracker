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
  readonly wrap?: boolean;
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
  wrap = false,
  spacing = 3,
}: CardResponsiveGridProps): JSX.Element {
  return (
    <Box
      sx={{
        display: wrap ? "flex" : "grid",
        flexWrap: wrap ? "wrap" : undefined,
        gap: spacing,
        width: contentSized ? { xs: "100%", sm: "max-content" } : undefined,
        justifyContent: "start",
        justifyItems: "stretch",
        alignItems: "start",
        gridTemplateColumns: wrap
          ? undefined
          : {
              xs: "minmax(0, 1fr)",
              sm:
                typeof columns === "number"
                  ? `repeat(${columns}, minmax(${minimumColumnWidth}px, max-content))`
                  : `repeat(auto-fit, minmax(${minimumColumnWidth}px, max-content))`,
            },
        "& > *": wrap
          ? {
              flex: `0 0 ${minimumColumnWidth}px`,
              minWidth: { xs: 0, sm: minimumColumnWidth },
              maxWidth: "100%",
            }
          : undefined,
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
