import { Drawer, Toolbar, Typography } from "@mui/material";
import type NavigationPage from "@core/fieldValues/NavigationPage";
import NavigationSelector from "@ui/navigation/NavigationSelector";
import appLogo from "/logo.svg";

/**
 * Props for the PermanentNavigation component.
 * @param {NavigationPage} initialPage - Initial page to be selected.
 * @param {Function} onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface PermanentNavigationProps {
  initialPage: NavigationPage;
  onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that presents the user with a navigation layout that is permanently on the screen.
 * @param {PermanentNavigationProps} props - Props for the PermanentNavigation component.
 * @returns {JSX.Element} JSX element representing the PermanentNavigation component.
 */
const PermanentNavigation = function ({
  initialPage,
  onNavigation,
}: PermanentNavigationProps): JSX.Element {
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

export default PermanentNavigation;
