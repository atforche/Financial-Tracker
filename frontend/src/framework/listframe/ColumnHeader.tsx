import { TableCell, TableSortLabel } from "@mui/material";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnSortType from "@framework/listframe/ColumnSortType";
import type { JSX } from "react";

/** Default width for a column in the list frame. */
const defaultColumnWidth = 100;

/**
 * Props for the ColumnHeader component.
 */
interface ColumnHeaderProps<T> {
  readonly column: ColumnDefinition<T>;
}

/**
 * Component that presents the header of a column in the list frame.
 * @param props - Props for the ColumnHeader component.
 * @returns JSX element representing the ColumnHeader.
 */
const ColumnHeader = function <T>({
  column,
}: ColumnHeaderProps<T>): JSX.Element {
  return (
    <TableCell
      key={column.name}
      align={column.alignment ?? "left"}
      sx={{
        maxWidth: column.maxWidth ?? defaultColumnWidth,
        backgroundColor: "primary.main",
        color: "white",
      }}
    >
      {(column.sortType ?? null) !== null || column.onSort ? (
        <TableSortLabel
          active={(column.sortType ?? null) !== null}
          direction={
            column.sortType === ColumnSortType.Ascending ? "asc" : "desc"
          }
          onClick={(): void => {
            if (column.sortType === null) {
              column.onSort?.(ColumnSortType.Ascending);
            } else if (column.sortType === ColumnSortType.Ascending) {
              column.onSort?.(ColumnSortType.Descending);
            } else {
              column.onSort?.(null);
            }
          }}
        >
          <div style={{ color: "white" }}>{column.headerContent}</div>
        </TableSortLabel>
      ) : (
        column.headerContent
      )}
    </TableCell>
  );
};

export default ColumnHeader;
