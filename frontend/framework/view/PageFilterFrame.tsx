import { Box, Paper, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";

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
}: PageFilterFrameProps): JSX.Element {
  return (
    <Paper
      sx={{
        position: sticky ? "sticky" : "relative",
        top: sticky ? 10 : undefined,
        zIndex: sticky ? (theme) : number | undefined => theme.zIndex.appBar - 1 : undefined,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
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
    </Paper>
  );
};

export type { PageFilterFrameProps };
export default PageFilterFrame;
