import { Box, type SxProps, type Theme, alpha } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Props for the InsetFrame component.
 */
interface InsetFrameProps {
  readonly children: ReactNode;
  readonly sx?: Exclude<SxProps<Theme>, readonly unknown[]>;
}

/**
 * Displays a lightweight, inset content frame with consistent application styling.
 */
const InsetFrame = function ({ children, sx }: InsetFrameProps): JSX.Element {
  return (
    <Box
      sx={[
        {
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 2,
          p: 1.5,
          backgroundColor: (theme: Theme) =>
            alpha(theme.palette.info.main, 0.04),
        },
        ...(sx === undefined ? [] : [sx]),
      ]}
    >
      {children}
    </Box>
  );
};

export type { InsetFrameProps };
export default InsetFrame;
