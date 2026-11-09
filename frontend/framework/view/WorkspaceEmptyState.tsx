import { Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";

/**
 * Props for the WorkspaceEmptyState component.
 */
interface WorkspaceEmptyStateProps {
  readonly title: string;
  readonly message: string;
}

/**
 * Explains why a workspace has nothing to show.
 */
const WorkspaceEmptyState = function ({
  title,
  message,
}: WorkspaceEmptyStateProps): JSX.Element {
  return (
    <ContentSurface>
      <Stack spacing={0.5}>
        <Typography variant="subtitle2">{title}</Typography>
        <Typography variant="body2" color="text.secondary">
          {message}
        </Typography>
      </Stack>
    </ContentSurface>
  );
};

export default WorkspaceEmptyState;
