import { Box, Paper, Stack, Typography, alpha } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Type representing the different colors that can be used for the Frame component.
 */
type FrameColor =
  "primary" | "secondary" | "success" | "info" | "warning" | "error";

/**
 * Props for the Frame component.
 */
interface FrameProps {
  readonly title: string;
  readonly children: ReactNode;
  readonly headerContent?: ReactNode;
  readonly color?: FrameColor;
}

/**
 * Gets the accent color for the provided frame color and theme.
 */
const getAccentColor = function (
  color: FrameColor,
  theme: {
    palette: {
      primary: { main: string };
      secondary: { main: string };
      success: { main: string };
      info: { main: string };
      warning: { main: string };
      error: { main: string };
    };
  },
): string {
  switch (color) {
    case "secondary":
      return theme.palette.secondary.main;
    case "success":
      return theme.palette.success.main;
    case "info":
      return theme.palette.info.main;
    case "warning":
      return theme.palette.warning.main;
    case "error":
      return theme.palette.error.main;
    case "primary":
    default:
      return theme.palette.primary.main;
  }
};

/**
 * Displays a shared content frame with a consistent application surface treatment.
 */
const Frame = function ({
  title,
  children,
  headerContent,
  color = "primary",
}: FrameProps): JSX.Element {
  return (
    <Paper
      sx={(theme) => ({
        position: "relative",
        overflow: "hidden",
        borderRadius: 5,
        border: `1px solid ${alpha(theme.palette.divider, 0.72)}`,
        backgroundColor: theme.palette.background.paper,
      })}
    >
      <Stack spacing={0}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
          sx={(theme) => {
            const accentColor = getAccentColor(color, theme);

            return {
              px: { xs: 2.5, md: 3 },
              py: 1.75,
              backgroundColor: alpha(accentColor, 0.08),
              borderBottom: `1px solid ${alpha(accentColor, 0.12)}`,
            };
          }}
        >
          <Stack direction="row" spacing={1.25} alignItems="center">
            <Box
              sx={(theme) => ({
                width: 11,
                height: 11,
                borderRadius: "50%",
                flexShrink: 0,
                backgroundColor: getAccentColor(color, theme),
                boxShadow: `0 0 0 5px ${alpha(getAccentColor(color, theme), 0.14)}`,
              })}
            />
            <Typography
              variant="h6"
              sx={(theme) => ({
                color: theme.palette.text.primary,
                overflowWrap: "anywhere",
              })}
            >
              {title}
            </Typography>
          </Stack>
          <Box sx={{ color: "text.primary" }}>{headerContent ?? null}</Box>
        </Stack>
        <Stack spacing={2.5} sx={{ p: { xs: 1, md: 1.5 } }}>
          {children}
        </Stack>
      </Stack>
    </Paper>
  );
};

export type { FrameColor, FrameProps };
export default Frame;
