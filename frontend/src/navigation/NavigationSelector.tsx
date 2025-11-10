import {
  Box,
  Divider,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import { type JSX, useState } from "react";
import {
  NavigationIcons,
  type NavigationPage,
  NavigationPages,
} from "@navigation/NavigationPage";

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
      <ListItem key={navigationPage} disablePadding>
        <ListItemButton
          selected={navigationPage === currentPage}
          onClick={() => {
            setCurrentPage(navigationPage);
            onNavigation(navigationPage);
          }}
        >
          <ListItemIcon sx={{ paddingLeft: "15px" }}>
            {NavigationIcons[navigationPage]}
          </ListItemIcon>
          <ListItemText primary={navigationPage} />
        </ListItemButton>
      </ListItem>
    );
  };

  return (
    <Box sx={{ overflow: "auto" }}>
      <Divider />
      <List>
        {NavigationPages.map((page) => buildNavigationElements(page))}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationSelector;
