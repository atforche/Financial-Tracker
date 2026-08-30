import type { JSX, ReactNode } from "react";
import { Paper } from "@mui/material";

/**
 * Props for the ContentSurface component.
 */
interface ContentSurfaceProps {
  readonly children: ReactNode;
  readonly sticky?: boolean;
  readonly mobileSticky?: boolean;
}

/**
 * Provides the standard bordered surface used for standalone page content.
 */
const ContentSurface = function ({
  children,
  sticky = false,
  mobileSticky = sticky,
}: ContentSurfaceProps): JSX.Element {
  return (
    <Paper
      sx={{
        position: {
          xs: mobileSticky ? "sticky" : "relative",
          lg: sticky ? "sticky" : "relative",
        },
        top: {
          xs: mobileSticky ? 10 : undefined,
          lg: sticky ? 10 : undefined,
        },
        zIndex:
          sticky || mobileSticky
            ? (theme): number => theme.zIndex.appBar - 1
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
