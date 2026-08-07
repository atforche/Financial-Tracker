"use client";

import {
  AppBar,
  Box,
  IconButton,
  Stack,
  Toolbar,
  Typography,
} from "@mui/material";
import { type JSX, type ReactNode, useState } from "react";
import ApplicationUserProvider from "@/framework/auth/ApplicationUserProvider";
import type { CurrentApplicationUser } from "@/framework/auth/currentApplicationUser";
import Image from "next/image";
import Menu from "@mui/icons-material/Menu";
import Navigation from "@/framework/navigation/Navigation";
import { usePathname } from "next/navigation";

/**
 * Props for the ApplicationShell component.
 */
interface ApplicationShellProps {
  readonly children: ReactNode;
  readonly applicationUser: CurrentApplicationUser | null;
  readonly user:
    | {
        readonly name?: string | null;
        readonly email?: string | null;
        readonly image?: string | null;
      }
    | undefined;
}

/**
 * Provides responsive application navigation and the main content region.
 */
const ApplicationShell = function ({
  applicationUser,
  children,
  user,
}: ApplicationShellProps): JSX.Element {
  const [mobileNavigationOpen, setMobileNavigationOpen] = useState(false);
  const pathname = usePathname();
  const isLoginPage = pathname === "/login";

  if (isLoginPage) {
    return (
      <Box component="main" sx={{ minHeight: "100vh" }}>
        {children}
      </Box>
    );
  }

  return (
    <ApplicationUserProvider user={applicationUser}>
      <Stack direction="row" sx={{ minHeight: "100vh" }}>
        <AppBar
          position="fixed"
          color="inherit"
          elevation={0}
          sx={{
            display: { xs: "block", md: "none" },
            zIndex: (theme) => theme.zIndex.drawer + 1,
          }}
        >
          <Toolbar>
            <IconButton
              aria-label="Open navigation"
              size="large"
              edge="start"
              color="inherit"
              onClick={() => {
                setMobileNavigationOpen(true);
              }}
              sx={{ mr: 2 }}
            >
              <Menu />
            </IconButton>
            <Image
              src="/icon.svg"
              height={60}
              width={60}
              alt="Financial Tracker Icon"
            />
            <Typography variant="h6" sx={{ marginLeft: 2 }}>
              Financial Tracker
            </Typography>
          </Toolbar>
        </AppBar>
        <Navigation
          applicationUser={applicationUser}
          user={user}
          visibility="desktop"
        />
        <Navigation
          applicationUser={applicationUser}
          user={user}
          variant="temporary"
          open={mobileNavigationOpen}
          onClose={() => {
            setMobileNavigationOpen(false);
          }}
          showBranding={false}
          visibility="mobile"
        />
        <Box
          component="main"
          sx={{ minWidth: 0, width: "100%", p: { xs: 2, sm: 3 } }}
        >
          <Toolbar sx={{ display: { xs: "flex", md: "none" } }} />
          {children}
        </Box>
      </Stack>
    </ApplicationUserProvider>
  );
};

export default ApplicationShell;
