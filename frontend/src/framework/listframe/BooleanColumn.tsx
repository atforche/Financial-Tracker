import { Checkbox, Skeleton } from "@mui/material";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import { type JSX } from "react";

/**
 * Props for the Boolean Column Header component.
 * @param {string} label - Label for the column.
 */
interface BooleanColumnHeaderProps {
  label: string;
}

/**
 * Component that renders the header for a boolean column.
 * @param {BooleanColumnHeaderProps} props - Props for the Boolean Column Header component.
 * @returns {JSX.Element} JSX element representing the Boolean Column Header.
 */
const BooleanColumnHeader = function ({
  label,
}: BooleanColumnHeaderProps): JSX.Element {
  return <ColumnHeader id={label} label={label} align="center" width={100} />;
};

/**
 * Props for the Boolean Column Cell component.
 * @param {string} label - Label for the column.
 * @param {boolean} value - Value to display in the cell.
 * @param {boolean} isLoading - Loading state of the cell.
 */
interface BooleanColumnCellProps {
  label: string;
  value: boolean;
  isLoading: boolean;
}

/**
 * Component that renders a cell for a boolean column.
 * @param {BooleanColumnCellProps} props - Props for the Boolean Column Cell component.
 * @returns {JSX.Element} JSX element representing the Boolean Column Cell.
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
