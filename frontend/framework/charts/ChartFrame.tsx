import { Box, Stack, Typography } from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX, ReactNode } from "react";
import { chartFontFamily } from "@/framework/charts/chartStyles";

/**
 * Props for the ChartFrame component.
 */
interface ChartFrameProps {
  readonly title: string;
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
  emptyMessage,
  hasData,
  children,
  xAxisLabel,
  yAxisLabel,
  color = "primary",
}: ChartFrameProps): JSX.Element {
  return (
    <Frame title={title} color={color}>
      <Box sx={{ p: { xs: 2, md: 1.5 } }}>
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
      </Box>
    </Frame>
  );
};

export default ChartFrame;
