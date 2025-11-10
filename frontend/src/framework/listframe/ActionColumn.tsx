import { Box, Button, IconButton, Skeleton } from "@mui/material";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import { type JSX } from "react";

/**
 * Props for the Action Column Header component.
 * @param {JSX.Element} icon - Icon to display in the header.
 * @param {string} caption - Caption for the header.
 * @param {Function} onClick - Callback to perform when the header is clicked.
 */
interface ActionColumnHeaderProps {
  icon: JSX.Element;
  caption: string;
  onClick: () => void;
}

/**
 * Component that renders the header for an action column.
 * @param {ActionColumnHeaderProps} props - Props for the Action Column Header component.
 * @returns {JSX.Element} JSX.Element representing the Action Column Header.
 */
const ActionColumnHeader = function ({
  icon,
  caption,
  onClick,
}: ActionColumnHeaderProps): JSX.Element {
  return (
    <ColumnHeader
      id="actions"
      label={
        <Box sx={{ verticalAlign: "middle" }}>
          <Button
            variant="contained"
            startIcon={icon}
            disableElevation
            sx={{ backgroundColor: "primary", border: 1, borderColor: "white" }}
            onClick={onClick}
          >
            {caption}
          </Button>
        </Box>
      }
      align="right"
      width={125}
    />
  );
};

/**
 * Props for the Action to be displayed in the Action Column.
 */
interface Action {
  icon: JSX.Element;
  onClick: () => void;
}

/**
 * Props for the Action Column Cell component.
 * @param {Action[]} actions - List of actions to display in the cell.
 * @param {boolean} isLoading - Loading state of the cell.
 */
interface ActionColumnCellProps {
  actions: Action[];
  isLoading: boolean;
}

/**
 * Component that renders a cell for an action column.
 * @param {ActionColumnCellProps} props - Props for the Action Column Cell component.
 * @returns {JSX.Element} JSX.Element representing the Action Column Cell.
 */
const ActionColumnCell = function ({
  actions,
  isLoading,
}: ActionColumnCellProps): JSX.Element {
  let cellValue: string | JSX.Element = (
    <>
      {actions.map((action, index) => (
        <IconButton key={index} onClick={action.onClick}>
          {action.icon}
        </IconButton>
      ))}
    </>
  );
  if (isLoading) {
    cellValue = <Skeleton />;
  }
  return (
    <ColumnCell id="actions" value={cellValue} align="right" width={125} />
  );
};

export { ActionColumnHeader, ActionColumnCell };
