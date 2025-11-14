import { type JSX, useCallback } from "react";
import {
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
} from "@mui/material";
import {
  NavigationIcons,
  type NavigationPage,
} from "@navigation/NavigationPage";

/**
 * Props for the NavigationElement component.
 * @param {NavigationPage} navigationPage - Navigation page represented by this element.
 * @param {boolean} isSelected - Whether or not this element is currently selected.
 * @param {(val: NavigationPage) => void} onSelect - Callback to be executed when this element is selected.
 */
interface NavigationElementProps {
  readonly navigationPage: NavigationPage;
  readonly isSelected: boolean;
  readonly onSelect: (val: NavigationPage) => void;
}

/**
 * Component that represents a single navigation element in the navigation selector.
 * @param {NavigationElementProps} props - Props for the NavigationElement component.
 * @returns {JSX.Element} JSX element representing the NavigationElement component.
 */
const NavigationElement = function ({
  navigationPage,
  isSelected,
  onSelect,
}: NavigationElementProps): JSX.Element {
  const onClick = useCallback(() => {
    onSelect(navigationPage);
  }, [navigationPage, onSelect]);

  return (
    <ListItem key={navigationPage} disablePadding>
      <ListItemButton selected={isSelected} onClick={onClick}>
        <ListItemIcon sx={{ paddingLeft: "15px" }}>
          {NavigationIcons[navigationPage]}
        </ListItemIcon>
        <ListItemText primary={navigationPage} />
      </ListItemButton>
    </ListItem>
  );
};

export default NavigationElement;
