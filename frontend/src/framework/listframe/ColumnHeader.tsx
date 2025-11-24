import type { JSX } from "react";
import TableCell from "@mui/material/TableCell";

/**
 * Props for the ColumnHeader component.
 * @param id - Id that uniquely identifies this Column Header.
 * @param label - Label for this Column Header.
 * @param align - Alignment for this Column Header.
 * @param width - Width for this Column Header.
 */
interface ColumnHeaderProps {
  readonly id: string;
  readonly label: string | JSX.Element;
  readonly align: "center" | "left" | "right";
  readonly width: number;
}

/**
 * Component for rendering a column header in a list frame.
 * @param props - Props for the ColumnHeader component.
 * @returns The column header element.
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
