import type { JSX, ReactNode } from "react";
import { Paper } from "@mui/material";

/**
 * Props for the ContentSurface component.
 */
interface ContentSurfaceProps {
  readonly children: ReactNode;
  readonly prominent?: boolean;
}

/**
 * Provides the standard bordered surface used for standalone page content.
 */
const ContentSurface = function ({
  children,
  prominent = false,
}: ContentSurfaceProps): JSX.Element {
  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: prominent ? { xs: 3, md: 4 } : 3,
      }}
    >
      {children}
    </Paper>
  );
};

export default ContentSurface;
