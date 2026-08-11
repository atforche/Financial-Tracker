import { Box, Paper, Stack, TableSortLabel, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";

/**
 * Props for the ListFrameMobileRow component.
 */
interface ListFrameMobileRowProps<T> {
  readonly columns: readonly ColumnDefinition<T>[];
  readonly getId: (item: T) => string;
  readonly item: T;
  readonly isRowSelected?: (item: T) => boolean;
  readonly onRowClick?: (item: T) => void;
}

/**
 * Renders a sortable column label in a mobile list-frame card.
 */
const getMobileColumnHeader = function <T>(
  column: ColumnDefinition<T>,
): ReactNode {
  if (column.onSort === undefined) {
    return column.headerContent;
  }

  const sortType = column.sortType ?? null;
  return (
    <TableSortLabel
      active={sortType !== null}
      direction={sortType === ColumnSortType.Ascending ? "asc" : "desc"}
      onClick={(event) => {
        event.stopPropagation();
        if (sortType === null) {
          column.onSort(ColumnSortType.Ascending);
        } else if (sortType === ColumnSortType.Ascending) {
          column.onSort(ColumnSortType.Descending);
        } else {
          column.onSort(null);
        }
      }}
      onKeyDown={(event) => {
        event.stopPropagation();
      }}
      sx={{
        color: "inherit",
        fontSize: "inherit",
        fontWeight: "inherit",
      }}
    >
      {column.headerContent}
    </TableSortLabel>
  );
};

/**
 * Displays a mobile list-frame value using the application body typography.
 */
const ListFrameMobileValue = function ({
  children,
  prominent = false,
}: {
  readonly children: ReactNode;
  readonly prominent?: boolean;
}): JSX.Element {
  return (
    <Typography
      component="div"
      variant="body2"
      sx={{
        color: "text.primary",
        minWidth: 0,
        overflowWrap: "anywhere",
        ...(prominent ? { fontWeight: 500 } : {}),
        "& > *": { maxWidth: "100%" },
      }}
    >
      {children}
    </Typography>
  );
};

/**
 * Displays one list-frame item as a mobile card.
 */
const ListFrameMobileRow = function <T>({
  columns,
  getId,
  isRowSelected,
  item,
  onRowClick,
}: ListFrameMobileRowProps<T>): JSX.Element {
  const labeledColumns = columns.filter(
    (column) =>
      column.headerContent !== null &&
      column.headerContent !== undefined &&
      column.headerContent !== "" &&
      column.headerContent !== false,
  );
  const primaryColumn =
    labeledColumns.find((column) => column.mobilePrimary === true) ??
    labeledColumns[0];
  const detailColumns = labeledColumns.filter(
    (column) => column !== primaryColumn,
  );
  const mobileColumns =
    primaryColumn === undefined
      ? detailColumns
      : [primaryColumn, ...detailColumns];
  const utilityColumns = columns.filter(
    (column) => !labeledColumns.includes(column),
  );
  const isClickable = typeof onRowClick === "function";
  const isSelected = isRowSelected?.(item) ?? false;
  const id = getId(item);

  return (
    <Paper
      variant="outlined"
      tabIndex={isClickable ? 0 : undefined}
      onClick={(): void => {
        if (isClickable) {
          onRowClick(item);
        }
      }}
      onKeyDown={(event): void => {
        if (isClickable && (event.key === "Enter" || event.key === " ")) {
          event.preventDefault();
          onRowClick(item);
        }
      }}
      sx={[
        {
          borderRadius: 2,
          minWidth: 0,
          p: 1.5,
          ...(isClickable ? { cursor: "pointer" } : {}),
        },
        isSelected
          ? {
              backgroundColor: "action.selected",
              borderColor: "primary.main",
            }
          : false,
      ]}
    >
      <Stack direction="row" spacing={1} alignItems="flex-start">
        <Box
          sx={{
            display: "grid",
            gap: 1.25,
            gridTemplateColumns: "repeat(auto-fit, minmax(260px, 1fr))",
            minWidth: 0,
            flex: 1,
          }}
        >
          {mobileColumns.map((column) => (
            <Box
              key={`${id}-${column.name}-mobile-field`}
              sx={{
                display: "grid",
                columnGap: 0.75,
                gridTemplateColumns: "7.5rem minmax(0, 1fr)",
                alignItems: "baseline",
                minWidth: 0,
              }}
            >
              <Typography
                variant="caption"
                sx={{
                  color: "text.secondary",
                  overflowWrap: "anywhere",
                }}
              >
                {getMobileColumnHeader(column)}
              </Typography>
              <Box sx={{ minWidth: 0 }}>
                <ListFrameMobileValue prominent={column === primaryColumn}>
                  {column.getBodyContent(item)}
                </ListFrameMobileValue>
              </Box>
            </Box>
          ))}
        </Box>
        {utilityColumns.length > 0 ? (
          <Stack
            direction="row"
            spacing={0.5}
            alignItems="center"
            sx={{ flexShrink: 0 }}
          >
            {utilityColumns.map((column) => (
              <Box
                key={`${id}-${column.name}-mobile-utility`}
                sx={{ display: "flex", alignItems: "center" }}
              >
                {column.getBodyContent(item)}
              </Box>
            ))}
          </Stack>
        ) : null}
      </Stack>
    </Paper>
  );
};

export default ListFrameMobileRow;
