import { Box, Stack, Typography } from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX, ReactNode } from "react";
import ContentSurface from "@/framework/view/ContentSurface";
import { chartFontFamily } from "@/framework/charts/chartStyles";

/**
 * How a chart is presented: in a frame with a colored header, or on a plain surface with its title inside.
 */
type ChartFrameVariant = "frame" | "surface";

/**
 * Props for the ChartFrame component.
 */
interface ChartFrameProps {
  readonly title: string;
  readonly headerContent?: ReactNode;
  readonly variant?: ChartFrameVariant;
  readonly emptyMessage: string;
  readonly hasData: boolean;
  readonly children: ReactNode;
  readonly xAxisLabel?: string;
  readonly yAxisLabel?: string;
  readonly color?: FrameColor;
}

/**
 * Provides consistent layout and empty-state presentation for charts.
 */
const ChartFrame = function ({
  title,
  headerContent,
  variant = "frame",
  emptyMessage,
  hasData,
  children,
  xAxisLabel,
  yAxisLabel,
  color = "primary",
}: ChartFrameProps): JSX.Element {
  const content = (
    <Stack spacing={hasData ? 2.5 : 1}>
      {hasData ? (
        <Box sx={{ display: "flex", flexDirection: "column", height: 320 }}>
          <Box sx={{ display: "flex", flex: 1, minHeight: 0 }}>
            {yAxisLabel === undefined ? null : (
              <Typography
                component="div"
                sx={{
                  alignItems: "center",
                  display: "flex",
                  fontFamily: chartFontFamily,
                  fontSize: 14,
                  justifyContent: "center",
                  left: -4,
                  lineHeight: 1,
                  position: "relative",
                  transform: "rotate(180deg)",
                  writingMode: "vertical-rl",
                }}
                variant="body2"
              >
                {yAxisLabel}
              </Typography>
            )}
            <Box
              aria-label={`${title} chart`}
              role="img"
              sx={{ flex: 1, minWidth: 0 }}
            >
              {children}
            </Box>
          </Box>
          {xAxisLabel === undefined ? null : (
            <Typography
              align="center"
              fontFamily={chartFontFamily}
              fontSize={14}
              lineHeight={1}
            >
              {xAxisLabel}
            </Typography>
          )}
        </Box>
      ) : (
        <Typography variant="body2" color="text.secondary">
          {emptyMessage}
        </Typography>
      )}
    </Stack>
  );

  if (variant === "surface") {
    return (
      <ContentSurface>
        <Stack spacing={2}>
          <Stack
            direction="row"
            spacing={1}
            sx={{ alignItems: "center", justifyContent: "space-between" }}
          >
            <Typography variant="h5">{title}</Typography>
            {headerContent}
          </Stack>
          {content}
        </Stack>
      </ContentSurface>
    );
  }

  return (
    <Frame title={title} headerContent={headerContent} color={color}>
      <Box sx={{ p: { xs: 2, md: 1.5 } }}>{content}</Box>
    </Frame>
  );
};

export type { ChartFrameVariant };
export default ChartFrame;
