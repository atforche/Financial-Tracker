import type { ChartColor } from "@/framework/charts/chartTypes";
import type { Theme } from "@mui/material/styles";

/**
 * Resolves a semantic chart color against the active MUI theme.
 */
const getChartColor = function (theme: Theme, color: ChartColor): string {
  switch (color) {
    case "secondary":
      return theme.palette.secondary.main;
    case "positive":
      return theme.palette.success.main;
    case "negative":
      return theme.palette.error.main;
    case "neutral":
      return theme.palette.grey[500];
    case "primary":
    default:
      return theme.palette.primary.main;
  }
};

export { getChartColor };
