"use client";

import { Box, IconButton, Stack, useMediaQuery, useTheme } from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import Menu from "@mui/icons-material/Menu";
import Navigation from "@/framework/navigation/Navigation";

/**
 * Props for the ApplicationShell component.
 */
interface ApplicationShellProps {
  readonly children: ReactNode;
}

/**
 * Provides responsive application navigation and the main content region.
 */
const ApplicationShell = function ({
  children,
}: ApplicationShellProps): JSX.Element {
  const theme = useTheme();
  const desktop = useMediaQuery(theme.breakpoints.up("md"));
  const [mobileNavigationOpen, setMobileNavigationOpen] = useState(false);

  return (
    <Stack direction="row" sx={{ minHeight: "100vh" }}>
      {desktop ? (
        <Navigation />
      ) : (
        <Navigation
          variant="temporary"
          open={mobileNavigationOpen}
          onClose={() => {
            setMobileNavigationOpen(false);
          }}
        />
      )}
      <Box
        component="main"
        sx={{ minWidth: 0, width: "100%", p: { xs: 2, sm: 3 } }}
      >
        {!desktop && (
          <IconButton
            aria-label="Open navigation"
            onClick={() => {
              setMobileNavigationOpen(true);
            }}
            sx={{ mb: 1 }}
          >
            <Menu />
          </IconButton>
        )}
        {children}
      </Box>
    </Stack>
  );
};

export default ApplicationShell;
