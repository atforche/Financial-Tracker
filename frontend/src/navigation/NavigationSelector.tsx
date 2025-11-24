import { Box, Divider, List } from "@mui/material";
import { type JSX, useState } from "react";
import {
  type NavigationPage,
  NavigationPages,
} from "@navigation/NavigationPage";
import NavigationElement from "./NavigationElement";

/**
 * Props for the NavigationSelector component.
 * @param initialPage - Initial page to be selected.
 * @param onNavigation - Callback to be executed whenever the navigation selection changes.
 */
interface NavigationSelectorProps {
  readonly initialPage: NavigationPage;
  readonly onNavigation: (val: NavigationPage) => void;
}

/**
 * Component that allows users to select which of the main navigation pages they want to navigate to.
 * @param props - Props for the NavigationSelector component.
 * @returns JSX element representing the NavigationSelector component.
 */
const NavigationSelector = function ({
  initialPage,
  onNavigation,
}: NavigationSelectorProps): JSX.Element {
  const [currentPage, setCurrentPage] = useState(initialPage);
  return (
    <Box sx={{ overflow: "auto" }}>
      <Divider />
      <List>
        {NavigationPages.map((page) => (
          <NavigationElement
            key={page}
            navigationPage={page}
            isSelected={page === currentPage}
            onSelect={(navigationPage) => {
              setCurrentPage(navigationPage);
              onNavigation(navigationPage);
            }}
          />
        ))}
      </List>
      <Divider />
    </Box>
  );
};

export default NavigationSelector;
