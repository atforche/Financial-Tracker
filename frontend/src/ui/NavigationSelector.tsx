import { AccountBalance, GridView } from "@mui/icons-material";
import {
  Box,
  Divider,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import NavigationPage from "./NavigationPage";
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

  return (
    <Box sx={{ overflow: "auto" }}>
      <List>
        {Object.entries(NavigationPage).map(([nameString, pageName]) => (
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
        ))}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationSelector;
