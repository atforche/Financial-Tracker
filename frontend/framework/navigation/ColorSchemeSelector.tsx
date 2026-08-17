"use client";

import {
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
  Typography,
} from "@mui/material";
import DarkModeOutlined from "@mui/icons-material/DarkModeOutlined";
import type { JSX } from "react";
import LightModeOutlined from "@mui/icons-material/LightModeOutlined";
import SettingsBrightnessOutlined from "@mui/icons-material/SettingsBrightnessOutlined";
import { useColorScheme } from "@mui/material/styles";

/**
 * Selects whether the application follows the system color scheme or uses a fixed scheme.
 */
const ColorSchemeSelector = function (): JSX.Element | null {
  const { mode, setMode } = useColorScheme();

  if (mode === undefined) {
    return null;
  }

  return (
    <Stack spacing={0.75} sx={{ mt: 2 }}>
      <Typography variant="caption" color="text.secondary">
        Color scheme
      </Typography>
      <ToggleButtonGroup
        aria-label="Color scheme"
        exclusive
        fullWidth
        size="small"
        value={mode}
        onChange={(_, nextMode: typeof mode | null) => {
          if (nextMode !== null) {
            setMode(nextMode);
          }
        }}
      >
        <ToggleButton value="system" aria-label="Use system color scheme">
          <Tooltip title="System">
            <SettingsBrightnessOutlined fontSize="small" />
          </Tooltip>
        </ToggleButton>
        <ToggleButton value="light" aria-label="Use light color scheme">
          <Tooltip title="Light">
            <LightModeOutlined fontSize="small" />
          </Tooltip>
        </ToggleButton>
        <ToggleButton value="dark" aria-label="Use dark color scheme">
          <Tooltip title="Dark">
            <DarkModeOutlined fontSize="small" />
          </Tooltip>
        </ToggleButton>
      </ToggleButtonGroup>
    </Stack>
  );
};

export default ColorSchemeSelector;
