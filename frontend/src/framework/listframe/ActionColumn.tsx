import { Box, Button, IconButton, Skeleton } from "@mui/material";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import type { JSX } from "react";

/**
 * Props for the Action Column Header component.
 * @param icon - Icon to display in the header.
 * @param caption - Caption for the header.
 * @param onClick - Callback to perform when the header is clicked.
 */
interface ActionColumnHeaderProps {
  readonly icon: JSX.Element;
  readonly caption: string;
  readonly onClick: () => void;
}

/**
 * Component that renders the header for an action column.
 * @param props - Props for the Action Column Header component.
 * @returns JSX.Element representing the Action Column Header.
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
  readonly key: string;
  icon: JSX.Element;
  handleClick: () => void;
}

/**
 * Props for the Action Column Cell component.
 * @param actions - List of actions to display in the cell.
 * @param isLoading - Loading state of the cell.
 * @param isError - Error state of the cell.
 */
interface ActionColumnCellProps {
  readonly actions: Action[];
  readonly isLoading: boolean;
  readonly isError: boolean;
}

/**
 * Component that renders a cell for an action column.
 * @param props - Props for the Action Column Cell component.
 * @returns JSX.Element representing the Action Column Cell.
 */
const ActionColumnCell = function ({
  actions,
  isLoading,
  isError,
}: ActionColumnCellProps): JSX.Element {
  let cellValue: string | JSX.Element = (
    <>
      {actions.map((action) => (
        <IconButton key={action.key} onClick={action.handleClick}>
          {action.icon}
        </IconButton>
      ))}
    </>
  );
  if (isLoading) {
    cellValue = <Skeleton />;
  }
  if (isError) {
    cellValue = <Skeleton animation={false} />;
  }
  return (
    <ColumnCell id="actions" value={cellValue} align="right" width={125} />
  );
};

export { ActionColumnHeader, ActionColumnCell };
