import type { JSX, ReactNode } from "react";
import { Paper, Stack, Typography } from "@mui/material";

/**
 * Props for the SummaryCard component.
 */
interface SummaryCardProps {
  readonly title: string;
  readonly value?: ReactNode;
  readonly description?: string;
  readonly children?: ReactNode;
}

/**
 * Displays a concise summary card with a title, primary value, and optional supporting content.
 */
const SummaryCard = function ({
  title,
  value,
  description,
  children,
}: SummaryCardProps): JSX.Element {
  return (
    <Paper
      sx={{
        p: 2,
        border: "1px solid",
        borderColor: "divider",
      }}
    >
      <Stack spacing={0.75}>
        <Typography variant="overline" color="text.secondary">
          {title}
        </Typography>
        {typeof value !== "undefined" && (
          <Typography variant="h4">{value}</Typography>
        )}
        {children}
        {typeof description !== "undefined" && (
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        )}
      </Stack>
    </Paper>
  );
};

export default SummaryCard;
