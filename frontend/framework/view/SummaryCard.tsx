import { Box, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import Frame from "@/framework/view/Frame";

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
    <Frame title={title}>
      <Stack spacing={0.75}>
        {typeof value !== "undefined" && (
          <Box sx={(theme) => theme.typography.h4}>{value}</Box>
        )}
        {children}
        {typeof description !== "undefined" && (
          <Typography variant="body2" color="text.secondary">
            {description}
          </Typography>
        )}
      </Stack>
    </Frame>
  );
};

export default SummaryCard;
