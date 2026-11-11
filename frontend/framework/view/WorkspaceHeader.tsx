import { Box, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Props for the WorkspaceHeader component.
 */
interface WorkspaceHeaderProps {
  readonly title: string;
  readonly action: ReactNode;
  readonly children: ReactNode;
}

/**
 * Displays a workspace's title and primary action above its filters.
 */
const WorkspaceHeader = function ({
  title,
  action,
  children,
}: WorkspaceHeaderProps): JSX.Element {
  return (
    <Stack spacing={2}>
      <Stack
        direction="row"
        useFlexGap
        spacing={2}
        // A long title wraps within its own space, so the action stays beside it
        sx={{ alignItems: "flex-start", justifyContent: "space-between" }}
      >
        <Typography variant="h4" sx={{ minWidth: 0, overflowWrap: "anywhere" }}>
          {title}
        </Typography>
        <Box sx={{ flexShrink: 0 }}>{action}</Box>
      </Stack>
      <Stack
        direction="row"
        useFlexGap
        spacing={1.5}
        sx={{ flexWrap: "wrap", alignItems: "center" }}
      >
        {children}
      </Stack>
    </Stack>
  );
};

export default WorkspaceHeader;
