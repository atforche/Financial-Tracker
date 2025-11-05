import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";

/**
 * Props for the String Column Header component.
 * @param {string} label - Label for the column.
 */
interface StringColumnHeaderProps {
  label: string;
}

/**
 * Component that renders the header for a string column.
 * @param {StringColumnHeaderProps} props - Props for the String Column Header component.
 * @returns {JSX.Element} JSX element representing the String Column Header.
 */
const StringColumnHeader = function ({
  label,
}: StringColumnHeaderProps): JSX.Element {
  return <ColumnHeader id={label} label={label} align="left" width={170} />;
};

/**
 * Props for the String Column Cell component.
 * @param {string} label - Label for the column.
 * @param {string | null} value - Value to display in the cell.
 */
interface StringColumnCellProps {
  label: string;
  value: string | null;
}

/**
 * Component that renders a cell for a string column.
 * @param {StringColumnCellProps} props - Props for the String Column Cell component.
 * @returns {JSX.Element} JSX element representing the String Column Cell.
 */
const StringColumnCell = function ({
  label,
  value,
}: StringColumnCellProps): JSX.Element {
  return <ColumnCell id={label} value={value ?? ""} align="left" width={170} />;
};

export { StringColumnHeader, StringColumnCell };
