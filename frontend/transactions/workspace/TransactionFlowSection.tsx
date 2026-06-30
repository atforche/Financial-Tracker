import { Box, Stack } from "@mui/material";
import EastOutlined from "@mui/icons-material/EastOutlined";
import type { JSX } from "react";
import SouthOutlined from "@mui/icons-material/SouthOutlined";
import TransactionSection from "@/transactions/workspace/TransactionSection";

/**
 * Props for the TransactionFlowSection component.
 */
interface TransactionFlowSectionProps {
  readonly title: string;
  readonly description: string;
  readonly sourceFrame: JSX.Element;
  readonly destinationFrames: JSX.Element[];
}

/**
 * Displays a transaction flow layout with a source, direction indicator, and destinations.
 */
const TransactionFlowSection = function ({
  title,
  description,
  sourceFrame,
  destinationFrames,
}: TransactionFlowSectionProps): JSX.Element {
  return (
    <TransactionSection title={title} description={description}>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          alignItems: "start",
          gridTemplateColumns: {
            xs: "1fr",
            lg: "minmax(280px, 0.95fr) auto minmax(320px, 1.15fr)",
          },
        }}
      >
        {sourceFrame}
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            minHeight: { lg: 88 },
            color: "text.secondary",
          }}
        >
          <EastOutlined
            sx={{
              display: { xs: "none", lg: "block" },
              fontSize: 40,
            }}
          />
          <SouthOutlined
            sx={{
              display: { xs: "block", lg: "none" },
              fontSize: 32,
            }}
          />
        </Box>
        <Stack spacing={2}>{destinationFrames}</Stack>
      </Box>
    </TransactionSection>
  );
};

export default TransactionFlowSection;
