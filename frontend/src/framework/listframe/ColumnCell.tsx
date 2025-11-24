import type { JSX } from "react";
import TableCell from "@mui/material/TableCell";

/**
 * Props for the ColumnCell component.
 * @param id - Id that uniquely identifies this Column Cell.
 * @param value - Value for this Column Cell.
 * @param align - Alignment for this Column Cell.
 * @param width - Width for this Column Cell.
 */
interface ColumnCellProps {
  readonly id: string;
  readonly value: string | JSX.Element;
  readonly align: "center" | "left" | "right";
  readonly width: number;
}

/**
 * Component for rendering a column cell in a list frame.
 * @param props - Props for the ColumnCell component.
 * @returns The column cell element.
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
