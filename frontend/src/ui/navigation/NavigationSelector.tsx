import { AccountBalance, GridView, Timeline } from "@mui/icons-material";
import {
  Box,
  Divider,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
} from "@mui/material";
import NavigationPage from "@core/fieldValues/NavigationPage";
import { useState } from "react";

/**
 * Given a NavigationPage, returns the icon component that should represent that NavigationPage.
 * @param {NavigationPage} page - NavigationPage to retrieve the icon for.
 * @returns {JSX.Element} The icon component for the provided NavigationPage.
 */
const getNavigationIcon = function (page: NavigationPage): JSX.Element {
  if (page === NavigationPage.Overview) {
    return <GridView key="Overview" />;
  }
  if (page === NavigationPage.AccountDashboard) {
    return <Timeline key="Accounts" />;
  }
  return <AccountBalance key="Accounts" />;
};

/**
 * Props for the NavigationSelector component.
 * @param {NavigationPage} initialPage - Initial page to be selected.
 * @param {Function} onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface NavigationSelectorProps {
  initialPage: NavigationPage;
  onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that allows users to select which of the main navigation pages they want to navigate to.
 * @param {NavigationSelectorProps} props - Props for the NavigationSelector component.
 * @returns {JSX.Element} JSX element representing the NavigationSelector component.
 */
export const NavigationSelector = function ({
  initialPage,
  onNavigation,
}: NavigationSelectorProps): JSX.Element {
  const [currentPage, setCurrentPage] = useState(initialPage);

  const buildNavigationElements = function (
    navigationPage: NavigationPage,
  ): JSX.Element {
    return (
      <ListItem key={navigationPage.toString()} disablePadding>
        <ListItemButton
          selected={navigationPage === currentPage}
          onClick={() => {
            setCurrentPage(navigationPage);
            onNavigation(navigationPage);
          }}
        >
          <ListItemIcon sx={{ paddingLeft: "15px" }}>
            {getNavigationIcon(navigationPage)}
          </ListItemIcon>
          <ListItemText primary={navigationPage.toString()} />
        </ListItemButton>
      </ListItem>
    );
  };

  return (
    <Box sx={{ overflow: "auto" }}>
      <List>
        <Divider>
          <Typography variant="body2">Dashboards</Typography>
        </Divider>
        {NavigationPage.Collection.filter((page) => !page.isDataEntry).map(
          (page) => buildNavigationElements(page),
        )}
        <Divider sx={{ paddingTop: "25px" }}>
          <Typography variant="body2">Data Entry</Typography>
        </Divider>
        {NavigationPage.Collection.filter((page) => page.isDataEntry).map(
          (page) => buildNavigationElements(page),
        )}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationSelector;
