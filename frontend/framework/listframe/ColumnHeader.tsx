import { Box, TableCell, TableSortLabel } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";

/**
 * Props for the ColumnHeader component.
 */
interface ColumnHeaderProps<T> {
  readonly column: ColumnDefinition<T>;
}

/**
 * Component that presents the header of a column in the list frame.
 */
const ColumnHeader = function <T>({
  column,
}: ColumnHeaderProps<T>): JSX.Element {
  return (
    <TableCell
      key={column.name}
      align={column.alignment ?? "left"}
      sx={[
        {
          width:
            column.minWidth === column.maxWidth ? column.minWidth : undefined,
          minWidth: 0,
          overflow: "hidden",
          textOverflow: "ellipsis",
          whiteSpace: "nowrap",
          backgroundColor: "primary.main",
          color: "white",
        },
        column.sx ?? false,
      ]}
    >
      {column.onSort ? (
        <TableSortLabel
          active={(column.sortType ?? null) !== null}
          direction={
            column.sortType === ColumnSortType.Ascending ? "asc" : "desc"
          }
          onClick={(): void => {
            const sortType = column.sortType ?? null;
            if (sortType === null) {
              column.onSort(ColumnSortType.Ascending);
            } else if (sortType === ColumnSortType.Ascending) {
              column.onSort(ColumnSortType.Descending);
            } else {
              column.onSort(null);
            }
          }}
          sx={{ maxWidth: "100%" }}
        >
          <Box
            component="span"
            sx={{
              minWidth: 0,
              overflow: "hidden",
              textOverflow: "ellipsis",
              whiteSpace: "nowrap",
            }}
          >
            {column.headerContent}
          </Box>
        </TableSortLabel>
      ) : (
        column.headerContent
      )}
    </TableCell>
  );
};

export default ColumnHeader;
