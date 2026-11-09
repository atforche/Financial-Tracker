import { Box, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import EastOutlined from "@mui/icons-material/EastOutlined";
import SouthOutlined from "@mui/icons-material/SouthOutlined";

/**
 * Props for the TransactionSourceDestinationLayout component.
 */
interface TransactionSourceDestinationLayoutProps {
  readonly sourceFrame: ReactNode;
  readonly destinationFrames: ReactNode;
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
        alignItems: "start",
        gridTemplateColumns: {
          xs: "1fr",
          lg: "minmax(280px, 0.95fr) auto minmax(320px, 1.15fr)",
        },
        // Wide screens give the source its own top row so the arrow centers on it beside the destinations
        gridTemplateRows: { lg: "auto 1fr" },
      }}
    >
      <Stack
        spacing={2}
        sx={{ position: "relative", zIndex: 1, gridRow: { lg: 1 } }}
      >
        {sourceFrame}
      </Stack>
      <EastOutlined
        sx={{
          display: { xs: "none", lg: "block" },
          fontSize: 30,
          gridRow: { lg: 1 },
          alignSelf: "center",
        }}
      />
      <SouthOutlined
        sx={{
          display: { xs: "block", lg: "none" },
          fontSize: 26,
          justifySelf: "center",
        }}
      />
      <Stack
        spacing={2}
        sx={{
          position: "relative",
          zIndex: 1,
          gridRow: { lg: "1 / span 2" },
          gridColumn: { lg: 3 },
        }}
      >
        {destinationFrames}
      </Stack>
    </Box>
  );
};

export default TransactionSourceDestinationLayout;
