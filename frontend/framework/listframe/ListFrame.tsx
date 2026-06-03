"use client";

import "@/framework/listframe/ListFrame.css";
import {
  Box,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeader from "@/framework/listframe/ColumnHeader";
import type { JSX } from "react";
import { rowsPerPage } from "@/framework/listframe/Constants";

/** Height of each row in the list frame. */
const listFrameRowHeight = 50;

/**
 * Information and actions for an explicit empty state.
 */
interface EmptyStateDefinition {
  readonly title: string;
  readonly description: string;
  readonly action: JSX.Element | null;
}

/**
 * Props for the ListFrame component.
 */
interface ListFrameProps<T> {
  readonly columns: ColumnDefinition<T>[];
  readonly getId: (item: T) => string;
  readonly data: T[] | null;
  readonly totalCount: number | null;
  readonly searchParamName: string;
  readonly pageParamName: string;
  readonly onRowClick?: (item: T) => void;
  readonly isRowSelected?: (item: T) => boolean;
  readonly hasActiveFilters?: boolean;
  readonly initialEmptyState?: EmptyStateDefinition;
  readonly filteredEmptyState?: EmptyStateDefinition;
}

/**
 * Component that presents a generic list frame with a table structure.
 */
const ListFrame = function <T>({
  columns,
  getId,
  data,
  totalCount,
  searchParamName,
  pageParamName,
  onRowClick,
  isRowSelected,
  hasActiveFilters,
  initialEmptyState,
  filteredEmptyState,
}: ListFrameProps<T>): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();
  const currentSearch = searchParams.get(searchParamName);
  const currentPage = searchParams.get(pageParamName);
  const isFiltered =
    typeof hasActiveFilters === "boolean"
      ? hasActiveFilters
      : typeof currentSearch === "string" && currentSearch.trim() !== "";

  const hasLoadingCompleted = data !== null && totalCount !== null;
  const numberOfRows = data?.length ?? 0;
  const placeholderRowCount =
    hasLoadingCompleted && numberOfRows > 0 ? rowsPerPage - numberOfRows : 0;

  let emptyStateToDisplay = null;
  if (hasLoadingCompleted && numberOfRows === 0) {
    if (isFiltered) {
      emptyStateToDisplay = filteredEmptyState ?? null;
    } else {
      emptyStateToDisplay = initialEmptyState ?? null;
    }
  }

  return (
    <Box>
      <Paper
        sx={{
          width: "100%",
        }}
      >
        <TableContainer>
          <Table stickyHeader>
            <TableHead>
              <TableRow>
                {columns.map((column) => (
                  <ColumnHeader key={column.name} column={column} />
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {hasLoadingCompleted
                ? data.map((item): JSX.Element => {
                    const isClickable = typeof onRowClick === "function";
                    const isSelected = isRowSelected?.(item) ?? false;
                    return (
                      <TableRow
                        hover
                        selected={isSelected}
                        tabIndex={-1}
                        key={getId(item)}
                        onClick={(): void => {
                          if (isClickable) {
                            onRowClick(item);
                          }
                        }}
                        style={{ height: listFrameRowHeight }}
                        sx={isClickable ? { cursor: "pointer" } : null}
                      >
                        {columns.map((column) => (
                          <TableCell
                            className="list-frame-table-cell"
                            key={`${getId(item)}-${column.name}`}
                            align={column.alignment ?? "left"}
                            sx={{
                              paddingTop: "8px",
                              paddingBottom: "8px",
                            }}
                          >
                            {column.getBodyContent(item)}
                          </TableCell>
                        ))}
                      </TableRow>
                    );
                  })
                : null}
              {placeholderRowCount > 0
                ? Array(placeholderRowCount)
                    .fill(null)
                    .map((_, index) => (
                      <TableRow
                        style={{ height: listFrameRowHeight }}
                        key={index}
                      >
                        {Array(columns.length)
                          .fill(null)
                          .map((__, cellIndex) => (
                            <TableCell
                              className="list-frame-table-cell"
                              key={`skeleton-${index}-${cellIndex}`}
                            />
                          ))}
                      </TableRow>
                    ))
                : null}
              {emptyStateToDisplay !== null ? (
                <TableRow>
                  <TableCell colSpan={columns.length}>
                    <Stack
                      spacing={1.5}
                      sx={{
                        alignItems: "center",
                        minHeight: rowsPerPage * listFrameRowHeight,
                        justifyContent: "center",
                        px: 3,
                        py: 4,
                        textAlign: "center",
                      }}
                    >
                      <Typography variant="h6">
                        {emptyStateToDisplay.title}
                      </Typography>
                      <Typography
                        variant="body2"
                        sx={{ color: "text.secondary", maxWidth: 420 }}
                      >
                        {emptyStateToDisplay.description}
                      </Typography>
                      {emptyStateToDisplay.action}
                    </Stack>
                  </TableCell>
                </TableRow>
              ) : null}
            </TableBody>
          </Table>
        </TableContainer>
        {hasLoadingCompleted && totalCount > 0 ? (
          <TablePagination
            rowsPerPageOptions={[rowsPerPage]}
            component="div"
            count={totalCount}
            rowsPerPage={rowsPerPage}
            page={currentPage === null ? 0 : parseInt(currentPage, 10) - 1}
            onPageChange={(_, newPage) => {
              const params = new URLSearchParams(searchParams.toString());
              params.set(pageParamName, (newPage + 1).toString());
              router.replace(`${pathname}?${params.toString()}`);
            }}
          />
        ) : null}
      </Paper>
    </Box>
  );
};

export default ListFrame;
export { rowsPerPage };
export type { EmptyStateDefinition };
