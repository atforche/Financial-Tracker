import { useMediaQuery, useTheme } from "@mui/material";

/**
 * Hook that determines if the current device is a mobile device based on screen size.
 * @returns True if the device is mobile, false otherwise.
 */
const useMobile = function (): boolean {
  const theme = useTheme();
  return useMediaQuery(theme.breakpoints.down("md"));
};

export default useMobile;
