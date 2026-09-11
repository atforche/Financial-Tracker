import { Box, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ContentSurface from "@/framework/view/ContentSurface";

/**
 * Props for the PageFilterFrame component.
 */
interface PageFilterFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly actions?: ReactNode;
  readonly mobileSticky?: boolean;
}

/**
 * Lays out page-level filter controls in a consistent application surface.
 */
const PageFilterFrame = function ({
  title,
  children,
  actions,
  mobileSticky = false,
}: PageFilterFrameProps): JSX.Element {
  return (
    <ContentSurface sticky mobileSticky={mobileSticky}>
      <Stack spacing={2}>
        <Stack
          direction={{ xs: "column", lg: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", lg: "center" }}
        >
          <Typography variant="h5">{title}</Typography>
        </Stack>
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

export type { PageFilterFrameProps };
export default PageFilterFrame;
