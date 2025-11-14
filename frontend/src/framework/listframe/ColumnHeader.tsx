import type { JSX } from "react";
import TableCell from "@mui/material/TableCell";

/**
 * Props for the ColumnHeader component.
 * @param {string} id - Id that uniquely identifies this Column Header.
 * @param {string | JSX.Element} label - Label for this Column Header.
 * @param {"center" | "left" | "right"} align - Alignment for this Column Header.
 * @param {number} width - Width for this Column Header.
 */
interface ColumnHeaderProps {
  readonly id: string;
  readonly label: string | JSX.Element;
  readonly align: "center" | "left" | "right";
  readonly width: number;
}

/**
 * Component for rendering a column header in a list frame.
 * @param {ColumnHeaderProps} props - Props for the ColumnHeader component.
 * @returns {JSX.Element} The column header element.
 */
const ColumnHeader = function ({
  id,
  label,
  align,
  width,
}: ColumnHeaderProps): JSX.Element {
  return (
    <TableCell
      key={id}
      align={align}
      style={{ minWidth: width }}
      sx={{ backgroundColor: "primary.main", color: "white" }}
    >
      {label}
    </TableCell>
  );
};

export default ColumnHeader;
