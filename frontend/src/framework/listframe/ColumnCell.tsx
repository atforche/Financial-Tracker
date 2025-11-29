import { Skeleton, TableCell } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the ColumnCell component.
 * @param key - Key that uniquely identifies this Column Cell.
 * @param content - Content for this Column Cell.
 * @param align - Alignment for this Column Cell.
 * @param isLoading - Loading state for this Column Cell.
 * @param isError - Error state for this Column Cell.
 */
interface ColumnCellProps {
  readonly key: string;
  readonly content: string | JSX.Element;
  readonly align: "center" | "left" | "right";
  readonly isLoading: boolean;
  readonly isError: boolean;
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
  isLoading,
  isError,
}: ColumnCellProps): JSX.Element {
  let cellContent: string | JSX.Element = content;
  if (isLoading) {
    cellContent = <Skeleton />;
  }
  if (isError) {
    cellContent = <Skeleton animation={false} />;
  }
  return (
    <TableCell key={key} align={align}>
      {cellContent}
    </TableCell>
  );
};

export default ColumnCell;
