import { Box, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ContentSurface from "@/framework/view/ContentSurface";

/**
 * Props for the PageFilterFrame component.
 */
interface PageFilterFrameProps {
  readonly children: ReactNode;
  readonly actions?: ReactNode;
  readonly mobileSticky?: boolean;
}

/**
 * Lays out page-level filter controls in a consistent application surface.
 */
const PageFilterFrame = function ({
  children,
  actions,
  mobileSticky = false,
}: PageFilterFrameProps): JSX.Element {
  return (
    <ContentSurface sticky mobileSticky={mobileSticky}>
      <Stack spacing={2} data-page-filters>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          {children}
          {actions === undefined ? null : (
            <Box sx={{ flexShrink: 0 }}>{actions}</Box>
          )}
        </Stack>
      </Stack>
    </ContentSurface>
  );
};

export default PageFilterFrame;
