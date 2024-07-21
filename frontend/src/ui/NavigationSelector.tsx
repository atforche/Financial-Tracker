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
import {
  DashboardPage,
  DataEntryPage,
  type NavigationPage,
} from "./NavigationPages";
import { useState } from "react";

/**
 * Given a NavigationPage, returns the icon component that should represent that NavigationPage.
 * @param {NavigationPage} page - NavigationPage to retrieve the icon for.
 * @returns {JSX.Element} The icon component for the provided NavigationPage.
 */
const getNavigationIcon = function (page: NavigationPage): JSX.Element {
  if (page === DashboardPage.Overview) {
    return <GridView key="Overview" />;
  }
  if (page === DashboardPage.Accounts) {
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
    nameString: string,
    pageName: NavigationPage,
  ): JSX.Element {
    return (
      <ListItem key={nameString} disablePadding>
        <ListItemButton
          selected={pageName === currentPage}
          onClick={() => {
            setCurrentPage(pageName);
            onNavigation(pageName);
          }}
        >
          <ListItemIcon sx={{ paddingLeft: "15px" }}>
            {getNavigationIcon(pageName)}
          </ListItemIcon>
          <ListItemText primary={nameString} />
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
        {Object.entries(DashboardPage).map(([nameString, pageName]) =>
          buildNavigationElements(nameString, pageName),
        )}
        <Divider sx={{ paddingTop: "25px" }}>
          <Typography variant="body2">Data Entry</Typography>
        </Divider>
        {Object.entries(DataEntryPage).map(([nameString, pageName]) =>
          buildNavigationElements(nameString, pageName),
        )}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationSelector;
