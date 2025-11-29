import type { JSX } from "react";
import TableCell from "@mui/material/TableCell";

/**
 * Props for the ColumnHeader component.
 * @param key - Key that uniquely identifies this Column Header.
 * @param content - Content for this Column Header.
 * @param align - Alignment for this Column Header.
 * @param width - Width for this Column Header.
 */
interface ColumnHeaderProps {
  readonly key: string;
  readonly content: string | JSX.Element;
  readonly align: "center" | "left" | "right";
  readonly minWidth: number;
}

/**
 * Component for rendering a column header in a list frame.
 * @param props - Props for the ColumnHeader component.
 * @returns The column header element.
 */
const ColumnHeader = function ({
  key,
  content,
  align,
  minWidth,
}: ColumnHeaderProps): JSX.Element {
  return (
    <TableCell
      key={key}
      align={align}
      style={{ minWidth }}
      sx={{ backgroundColor: "primary.main", color: "white" }}
    >
      {content}
    </TableCell>
  );
};

export default ColumnHeader;
