import { Box } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the AmountBar component.
 */
interface AmountBarProps {
  readonly ratio: number;
  readonly color: string;
  readonly height?: number | undefined;
  readonly borderRadius?: number | undefined;
}

/**
 * Displays a horizontal bar for a ratio between zero and one.
 */
const AmountBar = function ({
  ratio,
  color,
  height = 16,
  borderRadius = 1,
}: AmountBarProps): JSX.Element {
  const normalizedRatio = Math.min(Math.max(ratio, 0), 1);

  return (
    <Box
      sx={{
        width: "100%",
        height,
        borderRadius,
        backgroundColor: "divider",
        overflow: "hidden",
      }}
    >
      <Box
        sx={{
          width: `${Math.round(normalizedRatio * 100)}%`,
          height: "100%",
          backgroundColor: color,
          transition: "width 0.2s ease",
        }}
      />
    </Box>
  );
};

export type { AmountBarProps };
export default AmountBar;
