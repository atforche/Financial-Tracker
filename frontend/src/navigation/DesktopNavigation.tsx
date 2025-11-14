import { Drawer, Toolbar, Typography } from "@mui/material";
import type { JSX } from "react";
import type { NavigationPage } from "@navigation/NavigationPage";
import NavigationSelector from "@navigation/NavigationSelector";
import appLogo from "/logo.svg";
/**
 * Props for the DesktopNavigation component.
 * @param {NavigationPage} initialPage - Initial page to be selected.
 * @param {Function} onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface DesktopNavigationProps {
  readonly initialPage: NavigationPage;
  readonly onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that presents the user with a navigation layout that is permanently on the screen.
 * @param {DesktopNavigationProps} props - Props for the DesktopNavigation component.
 * @returns {JSX.Element} JSX element representing the DesktopNavigation component.
 */
const DesktopNavigation = function ({
  initialPage,
  onNavigation,
}: DesktopNavigationProps): JSX.Element {
  const drawerWidth = 260;
  return (
    <Drawer
      variant="permanent"
      sx={{
        [`& .MuiToolbar-root`]: { padding: "12px" },
        flexShrink: 0,
        width: drawerWidth,
      }}
    >
      <Toolbar>
        <img src={appLogo} height="60px" width="60px" />
        <Typography variant="h6" sx={{ marginLeft: 2 }}>
          Financial Tracker
        </Typography>
      </Toolbar>
      <NavigationSelector
        initialPage={initialPage}
        onNavigation={onNavigation}
      />
    </Drawer>
  );
};

export default DesktopNavigation;
