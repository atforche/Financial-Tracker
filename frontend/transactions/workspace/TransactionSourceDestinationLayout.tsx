import { Box, Stack } from "@mui/material";
import EastOutlined from "@mui/icons-material/EastOutlined";
import type { JSX } from "react";
import SouthOutlined from "@mui/icons-material/SouthOutlined";

/**
 * Props for the TransactionSourceDestinationLayout component.
 */
interface TransactionSourceDestinationLayoutProps {
  readonly sourceFrame: JSX.Element;
  readonly destinationFrames: JSX.Element[];
}

/**
 * Displays a transaction source destination layout with a source, direction indicator, and destinations.
 */
const TransactionSourceDestinationLayout = function ({
  sourceFrame,
  destinationFrames,
}: TransactionSourceDestinationLayoutProps): JSX.Element {
  return (
    <Box
      sx={{
        position: "relative",
        display: "grid",
        gap: 2,
        alignItems: { xs: "start", lg: "center" },
        gridTemplateColumns: {
          xs: "1fr",
          lg: "minmax(280px, 0.95fr) auto minmax(320px, 1.15fr)",
        },
      }}
    >
      <Box sx={{ position: "relative", zIndex: 1 }}>{sourceFrame}</Box>
      <EastOutlined
        sx={{
          display: { xs: "none", lg: "block" },
          fontSize: 30,
        }}
      />
      <SouthOutlined
        sx={{
          display: { xs: "block", lg: "none" },
          fontSize: 26,
          justifySelf: "center",
        }}
      />
      <Stack spacing={2} sx={{ position: "relative", zIndex: 1 }}>
        {destinationFrames}
      </Stack>
    </Box>
  );
};

export default TransactionSourceDestinationLayout;
