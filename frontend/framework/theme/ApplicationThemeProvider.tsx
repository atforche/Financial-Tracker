"use client";

import type { JSX, ReactNode } from "react";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";

/**
 * Props for the ApplicationThemeProvider component.
 */
interface ApplicationThemeProviderProps {
  readonly children: ReactNode;
}

/** The shared light and dark theme configuration for the application. */
const applicationTheme = createTheme({
  colorSchemes: {
    dark: true,
    light: true,
  },
  cssVariables: {
    colorSchemeSelector: "data",
  },
});

/**
 * Applies the selected application color scheme and global baseline styles.
 */
const ApplicationThemeProvider = function ({
  children,
}: ApplicationThemeProviderProps): JSX.Element {
  return (
    <ThemeProvider
      theme={applicationTheme}
      defaultMode="system"
      disableTransitionOnChange
    >
      <CssBaseline enableColorScheme />
      {children}
    </ThemeProvider>
  );
};

export default ApplicationThemeProvider;
