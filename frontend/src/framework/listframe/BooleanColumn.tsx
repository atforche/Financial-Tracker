import { Checkbox, Skeleton } from "@mui/material";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import type { JSX } from "react";

/**
 * Props for the Boolean Column Header component.
 * @param label - Label for the column.
 */
interface BooleanColumnHeaderProps {
  readonly label: string;
}

/**
 * Component that renders the header for a boolean column.
 * @param props - Props for the Boolean Column Header component.
 * @returns JSX element representing the Boolean Column Header.
 */
const BooleanColumnHeader = function ({
  label,
}: BooleanColumnHeaderProps): JSX.Element {
  return <ColumnHeader id={label} label={label} align="center" width={100} />;
};

/**
 * Props for the Boolean Column Cell component.
 * @param label - Label for the column.
 * @param value - Value to display in the cell.
 * @param isLoading - Loading state of the cell.
 */
interface BooleanColumnCellProps {
  readonly label: string;
  readonly value: boolean;
  readonly isLoading: boolean;
}

/**
 * Component that renders a cell for a boolean column.
 * @param props - Props for the Boolean Column Cell component.
 * @returns JSX element representing the Boolean Column Cell.
 */
const BooleanColumnCell = function ({
  label,
  value,
  isLoading,
}: BooleanColumnCellProps): JSX.Element {
  let cellValue: string | JSX.Element = value ? (
    <Checkbox disabled checked />
  ) : (
    <Checkbox disabled />
  );
  if (isLoading) {
    cellValue = <Skeleton />;
  }
  return <ColumnCell id={label} value={cellValue} align="center" width={100} />;
};

export { BooleanColumnHeader, BooleanColumnCell };
