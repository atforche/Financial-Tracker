import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import type { JSX } from "react";
import Skeleton from "@mui/material/Skeleton";

/**
 * Props for the String Column Header component.
 * @param label - Label for the column.
 */
interface StringColumnHeaderProps {
  readonly label: string;
}

/**
 * Component that renders the header for a string column.
 * @param props - Props for the String Column Header component.
 * @returns JSX element representing the String Column Header.
 */
const StringColumnHeader = function ({
  label,
}: StringColumnHeaderProps): JSX.Element {
  return <ColumnHeader id={label} label={label} align="left" width={170} />;
};

/**
 * Props for the String Column Cell component.
 * @param label - Label for the column.
 * @param value - Value to display in the cell.
 * @param isLoading - Loading state of the cell.
 * @param isError - Error state of the cell.
 */
interface StringColumnCellProps {
  readonly label: string;
  readonly value: string | null;
  readonly isLoading: boolean;
  readonly isError: boolean;
}

/**
 * Component that renders a cell for a string column.
 * @param props - Props for the String Column Cell component.
 * @returns JSX element representing the String Column Cell.
 */
const StringColumnCell = function ({
  label,
  value,
  isLoading,
  isError,
}: StringColumnCellProps): JSX.Element {
  let cellValue: string | JSX.Element = value ?? "";
  if (isLoading) {
    cellValue = <Skeleton />;
  }
  if (isError) {
    cellValue = <Skeleton animation={false} />;
  }
  return <ColumnCell id={label} value={cellValue} align="left" width={170} />;
};

export { StringColumnHeader, StringColumnCell };
