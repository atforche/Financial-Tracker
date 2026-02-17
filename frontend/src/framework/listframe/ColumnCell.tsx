import type { JSX } from "react";
import { TableCell } from "@mui/material";

/**
 * Props for the ColumnCell component.
 * @param key - Key that uniquely identifies this Column Cell.
 * @param content - Content for this Column Cell.
 * @param align - Alignment for this Column Cell.
 */
interface ColumnCellProps {
  readonly key: string;
  readonly content: string | JSX.Element;
  readonly align: "center" | "left" | "right";
}

/**
 * Component for rendering a column cell in a list frame.
 * @param props - Props for the ColumnCell component.
 * @returns The column cell element.
 */
const ColumnCell = function ({
  key,
  content,
  align,
}: ColumnCellProps): JSX.Element {
  return (
    <TableCell key={key} align={align}>
      {content}
    </TableCell>
  );
};

export default ColumnCell;
