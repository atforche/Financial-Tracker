import { Box, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ContentSurface from "@/framework/view/ContentSurface";

/**
 * Props for the PageFilterFrame component.
 */
interface PageFilterFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly headerContent?: ReactNode;
  readonly actions?: ReactNode;
  readonly description?: ReactNode;
  readonly sticky?: boolean;
  readonly mobileSticky?: boolean;
}

/**
 * Lays out page-level filter controls in a consistent application surface.
 */
const PageFilterFrame = function ({
  title,
  children,
  headerContent,
  actions,
  description,
  sticky = true,
  mobileSticky = sticky,
}: PageFilterFrameProps): JSX.Element {
  return (
    <ContentSurface sticky={sticky} mobileSticky={mobileSticky}>
      <Stack spacing={2}>
        <Stack
          direction={{ xs: "column", lg: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", lg: "center" }}
        >
          <Stack spacing={0.5}>
            <Typography variant="h5">{title}</Typography>
            {description === undefined ? null : (
              <Typography color="text.secondary">{description}</Typography>
            )}
          </Stack>
          {headerContent === undefined ? null : (
            <Box sx={{ flexShrink: 0 }}>{headerContent}</Box>
          )}
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
