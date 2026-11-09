import type { JSX, ReactNode } from "react";
import { Stack, Typography } from "@mui/material";

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
        sx={{
          flexWrap: "wrap",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <Typography variant="h4">{title}</Typography>
        {action}
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
