import type { JSX } from "react";
import TableCell from "@mui/material/TableCell";

/**
 * Props for the ColumnCell component.
 * @param {string} id - Id that uniquely identifies this Column Cell.
 * @param {string | JSX.Element} value - Value for this Column Cell.
 * @param {"center" | "left" | "right"} align - Alignment for this Column Cell.
 * @param {number} width - Width for this Column Cell.
 */
interface ColumnCellProps {
  id: string;
  value: string | JSX.Element;
  align: "center" | "left" | "right";
  width: number;
}

/**
 * Component for rendering a column cell in a list frame.
 * @param {ColumnCellProps} props - Props for the ColumnCell component.
 * @returns {JSX.Element} The column cell element.
 */
const ColumnCell = function ({
  id,
  value,
  align,
  width,
}: ColumnCellProps): JSX.Element {
  return (
    <TableCell key={id} align={align} style={{ minWidth: width }}>
      {value}
    </TableCell>
  );
};

export default ColumnCell;
