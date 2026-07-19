import { Paper, Stack, Typography } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the ChartTooltip component.
 */
interface ChartTooltipProps {
  readonly label: string;
  readonly value: string;
  readonly description?: string | undefined;
}

/**
 * Displays the shared chart tooltip presentation.
 */
const ChartTooltip = function ({
  label,
  value,
  description,
}: ChartTooltipProps): JSX.Element {
  return (
    <Paper
      elevation={3}
      sx={{
        border: "1px solid",
        borderColor: "divider",
        minWidth: 180,
        p: 1.5,
      }}
    >
      <Stack spacing={0.5}>
        <Typography variant="overline" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="body1">{value}</Typography>
        {typeof description === "undefined" ? null : (
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        )}
      </Stack>
    </Paper>
  );
};

export default ChartTooltip;
