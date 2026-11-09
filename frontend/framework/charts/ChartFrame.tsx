import { Box, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ContentSurface from "@/framework/view/ContentSurface";
import { chartFontFamily } from "@/framework/charts/chartStyles";

/**
 * Props for the ChartFrame component.
 */
interface ChartFrameProps {
  readonly title: string;
  readonly headerContent?: ReactNode;
  readonly emptyMessage: string;
  readonly hasData: boolean;
  readonly children: ReactNode;
  readonly xAxisLabel?: string;
  readonly yAxisLabel?: string;
}

/**
 * Provides consistent layout and empty-state presentation for charts.
 */
const ChartFrame = function ({
  title,
  headerContent,
  emptyMessage,
  hasData,
  children,
  xAxisLabel,
  yAxisLabel,
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
};

export default ChartFrame;
