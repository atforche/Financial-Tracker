import { Box } from "@mui/material";
import type { JSX } from "react";

/**
 * One segment of a segmented bar. A segment without a color leaves the track showing.
 */
interface SegmentedBarSegment {
  readonly key: string;
  readonly amount: number;
  readonly color?: string;
}

/**
 * Props for the SegmentedBar component.
 */
interface SegmentedBarProps {
  readonly segments: readonly SegmentedBarSegment[];
  readonly total?: number;
}

/**
 * Displays segments side by side in one rounded bar, each as its share of the total, which defaults to the segments'
 * sum.
 */
const SegmentedBar = function ({
  segments,
  total = segments.reduce((sum, segment) => sum + segment.amount, 0),
}: SegmentedBarProps): JSX.Element {
  return (
    <Box
      sx={{
        display: "flex",
        height: 12,
        borderRadius: 6,
        overflow: "hidden",
        bgcolor: "action.hover",
      }}
    >
      {segments.map((segment) => (
        <Box
          key={segment.key}
          sx={{
            width: `${total > 0 ? (segment.amount / total) * 100 : 0}%`,
            bgcolor: segment.color,
          }}
        />
      ))}
    </Box>
  );
};

export default SegmentedBar;
