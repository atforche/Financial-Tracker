import {
  AppBar,
  Box,
  Drawer,
  IconButton,
  Toolbar,
  Typography,
  useTheme,
} from "@mui/material";
import { type JSX, useCallback, useState } from "react";
import MenuIcon from "@mui/icons-material/Menu";
import type { NavigationPage } from "@navigation/NavigationPage";
import NavigationSelector from "@navigation/NavigationSelector";
import appLogo from "@public/logo.svg";

/**
 * Props for the MobileNavigation component.
 * @param {NavigationPage} initialPage - Initial page to be selected.
 * @param {(val: NavigationPage) => void} onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface MobileNavigationProps {
  readonly initialPage: NavigationPage;
  readonly onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that presents the user with a navigation layout for mobile devices.
 * @param {MobileNavigationProps} props - Props for the MobileNavigation component.
 * @returns {JSX.Element} JSX element representing the MobileNavigation component.
 */
const MobileNavigation = function ({
  initialPage,
  onNavigation,
}: MobileNavigationProps): JSX.Element {
  const theme = useTheme();
  const zIndexIncrement = 1;
  const drawerWidth = 260;
  const [mobileOpen, setMobileOpen] = useState(false);
  const [isClosing, setIsClosing] = useState(false);

  const handleDrawerClose = useCallback((): void => {
    setIsClosing(true);
    setMobileOpen(false);
  }, []);

  const handleDrawerTransitionEnd = useCallback((): void => {
    setIsClosing(false);
  }, []);

  const handleDrawerToggle = useCallback((): void => {
    if (!isClosing) {
      setMobileOpen(!mobileOpen);
    }
  }, [isClosing, mobileOpen]);

  return (
    <Box>
      <AppBar
        position="fixed"
        color="inherit"
        elevation={0}
        sx={{ zIndex: theme.zIndex.drawer + zIndexIncrement }}
      >
        <Toolbar>
          <IconButton size="large" edge="start" color="inherit" sx={{ mr: 2 }}>
            <MenuIcon onClick={handleDrawerToggle} />
          </IconButton>
          <img src={appLogo} height="60px" width="60px" />
          <Typography variant="h6" sx={{ marginLeft: 2 }}>
            Financial Tracker
          </Typography>
        </Toolbar>
      </AppBar>
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onTransitionEnd={handleDrawerTransitionEnd}
        onClose={handleDrawerClose}
        ModalProps={{
          keepMounted: true,
        }}
        sx={{
          [`& .MuiDrawer-paper`]: {
            boxSizing: "border-box",
            width: drawerWidth,
          },
          [`& .MuiToolbar-root`]: { padding: "12px" },
          flexShrink: 0,
          width: drawerWidth,
        }}
      >
        <Toolbar />
        <NavigationSelector
          initialPage={initialPage}
          onNavigation={onNavigation}
        />
      </Drawer>
      <Toolbar />
    </Box>
  );
};

export default MobileNavigation;
