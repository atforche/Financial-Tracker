import {
  AppBar,
  Box,
  Drawer,
  IconButton,
  Toolbar,
  Typography,
  useTheme,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import type NavigationPage from "@core/fieldValues/NavigationPage";
import NavigationSelector from "@ui/navigation/NavigationSelector";
import appLogo from "/logo.svg";
import { useState } from "react";

/**
 * Props for the TemporaryNavigation component.
 * @param {NavigationPage} initialPage - Initial page to be selected.
 * @param {Function} onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface TemporaryNavigationProps {
  initialPage: NavigationPage;
  onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that presents the user with a navigation layout that is only temporarily on the screen.
 * @param {TemporaryNavigationProps} props - Props for the TemporaryNavigation component.
 * @returns {JSX.Element} JSX element representing the TemporaryNavigation component.
 */
const TemporaryNavigation = function ({
  initialPage,
  onNavigation,
}: TemporaryNavigationProps): JSX.Element {
  const theme = useTheme();
  const zIndexIncrement = 1;
  const drawerWidth = 260;
  const [mobileOpen, setMobileOpen] = useState(false);
  const [isClosing, setIsClosing] = useState(false);

  const handleDrawerClose = (): void => {
    setIsClosing(true);
    setMobileOpen(false);
  };

  const handleDrawerTransitionEnd = (): void => {
    setIsClosing(false);
  };

  const handleDrawerToggle = (): void => {
    if (!isClosing) {
      setMobileOpen(!mobileOpen);
    }
  };

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

export default TemporaryNavigation;
