import { IconButton, Paper, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import DeleteOutline from "@mui/icons-material/DeleteOutline";

interface TransactionFrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly description?: string | null;
  readonly onRemove?: (() => void) | null;
}

/**
 * Displays a framed source or destination model inside a transaction form.
 */
const TransactionFrame = function ({
  title,
  children,
  description = null,
  onRemove = null,
}: TransactionFrameProps): JSX.Element {
  return (
    <Paper
      variant="outlined"
      sx={{
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
        backgroundColor: "background.default",
      }}
    >
      <Stack spacing={2}>
        <Stack
          direction="row"
          spacing={1}
          justifyContent="space-between"
          alignItems="flex-start"
        >
          <Stack spacing={0.5}>
            <Typography variant="subtitle1" fontWeight={700}>
              {title}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {description}
            </Typography>
          </Stack>
          {onRemove === null ? null : (
            <IconButton size="small" color="error" onClick={onRemove}>
              <DeleteOutline fontSize="small" />
            </IconButton>
          )}
        </Stack>
        {children}
      </Stack>
    </Paper>
  );
};

export default TransactionFrame;
