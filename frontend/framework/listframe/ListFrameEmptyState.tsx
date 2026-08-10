import type { JSX, ReactNode } from "react";
import { Stack, Typography } from "@mui/material";

/**
 * Information and actions for an explicit empty state.
 */
interface EmptyStateDefinition {
  readonly title: string;
  readonly description: string;
  readonly action?: ReactNode;
}

/**
 * Props for the ListFrameEmptyState component.
 */
interface ListFrameEmptyStateProps {
  readonly emptyState: EmptyStateDefinition;
  readonly desktopMinHeight: number;
}

/**
 * Displays an empty state inside either the desktop table or mobile card list.
 */
const ListFrameEmptyState = function ({
  emptyState,
  desktopMinHeight,
}: ListFrameEmptyStateProps): JSX.Element {
  return (
    <Stack
      spacing={1.5}
      sx={{
        alignItems: "center",
        minHeight: { xs: 260, md: desktopMinHeight },
        justifyContent: "center",
        px: 3,
        py: 4,
        textAlign: "center",
      }}
    >
      <Typography variant="h6">{emptyState.title}</Typography>
      <Typography
        variant="body2"
        sx={{ color: "text.secondary", maxWidth: 420 }}
      >
        {emptyState.description}
      </Typography>
      {emptyState.action}
    </Stack>
  );
};

export type { EmptyStateDefinition };
export default ListFrameEmptyState;
