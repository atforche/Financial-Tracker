import type { JSX } from "react";
import TableCell from "@mui/material/TableCell";

const defaultMaxWidth = 100;

/**
 * Props for the ColumnHeader component.
 */
interface ColumnHeaderProps {
  readonly key: string;
  readonly content: string | JSX.Element;
  readonly align: "center" | "left" | "right";
  readonly maxWidth?: number;
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
  maxWidth = defaultMaxWidth,
}: ColumnHeaderProps): JSX.Element {
  return (
    <TableCell
      key={key}
      align={align}
      style={{ maxWidth }}
      sx={{ backgroundColor: "primary.main", color: "white" }}
    >
      {content}
    </TableCell>
  );
};

export default ColumnHeader;
