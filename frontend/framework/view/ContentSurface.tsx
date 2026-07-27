import type { JSX, ReactNode } from "react";
import { Paper } from "@mui/material";

/**
 * Props for the ContentSurface component.
 */
interface ContentSurfaceProps {
  readonly children: ReactNode;
  readonly sticky?: boolean;
}

/**
 * Provides the standard bordered surface used for standalone page content.
 */
const ContentSurface = function ({
  children,
  sticky = false,
}: ContentSurfaceProps): JSX.Element {
  return (
    <Paper
      sx={{
        position: sticky ? "sticky" : "relative",
        top: sticky ? 10 : undefined,
        zIndex: sticky
          ? (theme): number | undefined => theme.zIndex.appBar - 1
          : undefined,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      {children}
    </Paper>
  );
};

export default ContentSurface;
