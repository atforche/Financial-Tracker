import { Box, Paper, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Props for the ChartFrame component.
 */
interface ChartFrameProps {
  readonly title: string;
  readonly emptyMessage: string;
  readonly hasData: boolean;
  readonly children: ReactNode;
}

/**
 * Provides consistent layout and empty-state presentation for charts.
 */
const ChartFrame = function ({
  title,
  emptyMessage,
  hasData,
  children,
}: ChartFrameProps): JSX.Element {
  return (
    <Paper sx={{ border: "1px solid", borderColor: "divider", p: 3 }}>
      <Stack spacing={hasData ? 2.5 : 1}>
        <Typography variant="h5">{title}</Typography>
        {hasData ? (
          <Box
            aria-label={`${title} chart`}
            role="img"
            sx={{ height: 320, width: "100%" }}
          >
            {children}
          </Box>
        ) : (
          <Typography variant="body2" color="text.secondary">
            {emptyMessage}
          </Typography>
        )}
      </Stack>
    </Paper>
  );
};

export default ChartFrame;
