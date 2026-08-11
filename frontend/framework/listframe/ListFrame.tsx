"use client";

import "@/framework/listframe/ListFrame.css";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import { type JSX, type ReactNode, useEffect } from "react";
import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  useMediaQuery,
} from "@mui/material";
import {
  desktopRowsPerPage,
  getPaginationIndex,
  getRowsPerPage,
  mobileRowsPerPage,
} from "@/framework/listframe/page";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeader from "@/framework/listframe/ColumnHeader";
import ListFrameEmptyState from "@/framework/listframe/ListFrameEmptyState";
import ListFrameMobile from "@/framework/listframe/ListFrameMobile";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import { useTheme } from "@mui/material/styles";

/** Height of each row in the list frame. */
const listFrameRowHeight = 50;

/**
 * Information and actions for an explicit empty state.
 */
interface EmptyStateDefinition {
  readonly title: string;
  readonly description: string;
  readonly action?: ReactNode;
}

/**
 * Props for the ListFrame component.
 */
interface ListFrameProps<T> {
  readonly title: string;
  /** The viewport width at which the fixed-width desktop table is suitable. */
  readonly desktopBreakpoint?: "sm" | "md" | "lg" | "xl";
  readonly headerContent?: ReactNode;
  readonly color?: FrameColor;
  readonly columns: readonly ColumnDefinition<T>[];
  readonly getId: (item: T) => string;
  readonly data: readonly T[] | null;
  readonly totalCount: number | null;
  readonly searchParamName?: string;
  readonly pageParamName: string;
  readonly onRowClick?: (item: T) => void;
  readonly isRowSelected?: (item: T) => boolean;
  readonly hasActiveFilters?: boolean;
  readonly initialEmptyState?: EmptyStateDefinition;
  readonly filteredEmptyState?: EmptyStateDefinition;
}

/**
 * Component that presents a generic list frame with table and mobile card layouts.
 */
const ListFrame = function <T>({
  title,
  desktopBreakpoint = "lg",
  headerContent,
  color = "primary",
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
  const updateParams = useSearchParamUpdater([]);
  const theme = useTheme();
  const isDesktopLayout = useMediaQuery(
    theme.breakpoints.up(desktopBreakpoint),
    { noSsr: true },
  );
  const currentSearch =
    typeof searchParamName === "string"
      ? searchParams.get(searchParamName)
      : null;
  const currentPage = searchParams.get(pageParamName);
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const paginationIndex = getPaginationIndex(
    currentPage,
    totalCount ?? 0,
    rowsPerPage,
  );
  const isFiltered =
    typeof hasActiveFilters === "boolean"
      ? hasActiveFilters
      : typeof currentSearch === "string" && currentSearch.trim() !== "";

  const hasLoadingCompleted = data !== null && totalCount !== null;
  const desktopLayoutSx = {
    display: { xs: "none", [desktopBreakpoint]: "block" },
    overflowX: "hidden",
  };
  const numberOfRows = data?.length ?? 0;
  const placeholderRowCount =
    hasLoadingCompleted && numberOfRows > 0 ? rowsPerPage - numberOfRows : 0;

  useEffect(() => {
    if (!hasLoadingCompleted) {
      return;
    }
    const canonicalPage = (paginationIndex + 1).toString();
    if (currentPage !== null && currentPage !== canonicalPage) {
      updateParams((params) => {
        params.set(pageParamName, canonicalPage);
      });
    }
  }, [
    currentPage,
    hasLoadingCompleted,
    pageParamName,
    paginationIndex,
    updateParams,
  ]);

  useEffect(() => {
    const expectedRowsPerPage = isDesktopLayout
      ? desktopRowsPerPage
      : mobileRowsPerPage;
    if (rowsPerPage !== expectedRowsPerPage) {
      updateParams((params) => {
        params.set("pageSize", expectedRowsPerPage.toString());
      });
    }
  }, [isDesktopLayout, rowsPerPage, updateParams]);

  let emptyStateToDisplay = null;
  if (hasLoadingCompleted && numberOfRows === 0) {
    if (isFiltered) {
      emptyStateToDisplay = filteredEmptyState ?? null;
    } else {
      emptyStateToDisplay = initialEmptyState ?? null;
    }
  }

  return (
    <Frame title={title} headerContent={headerContent} color={color}>
      <Paper
        sx={{
          width: "100%",
          borderRadius: 3,
          overflow: "hidden",
        }}
      >
        <TableContainer sx={desktopLayoutSx}>
          <Table stickyHeader sx={{ tableLayout: "fixed", width: "100%" }}>
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
                    const id = getId(item);
                    return (
                      <TableRow
                        hover
                        selected={isSelected}
                        tabIndex={isClickable ? 0 : undefined}
                        key={id}
                        onClick={(): void => {
                          if (isClickable) {
                            onRowClick(item);
                          }
                        }}
                        onKeyDown={(event): void => {
                          if (
                            isClickable &&
                            (event.key === "Enter" || event.key === " ")
                          ) {
                            event.preventDefault();
                            onRowClick(item);
                          }
                        }}
                        style={{ height: listFrameRowHeight }}
                        sx={isClickable ? { cursor: "pointer" } : null}
                      >
                        {columns.map((column) => (
                          <TableCell
                            className="list-frame-table-cell"
                            key={`${id}-${column.name}`}
                            align={column.alignment ?? "left"}
                            sx={[
                              {
                                minWidth: 0,
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap",
                                paddingTop: "8px",
                                paddingBottom: "8px",
                              },
                              column.sx ?? false,
                            ]}
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
                              sx={columns[cellIndex]?.sx}
                            />
                          ))}
                      </TableRow>
                    ))
                : null}
              {emptyStateToDisplay !== null ? (
                <TableRow>
                  <TableCell colSpan={columns.length}>
                    <ListFrameEmptyState
                      desktopMinHeight={rowsPerPage * listFrameRowHeight}
                      emptyState={emptyStateToDisplay}
                    />
                  </TableCell>
                </TableRow>
              ) : null}
            </TableBody>
          </Table>
        </TableContainer>
        <ListFrameMobile
          columns={columns}
          data={data}
          emptyState={
            emptyStateToDisplay === null ? null : (
              <ListFrameEmptyState
                desktopMinHeight={rowsPerPage * listFrameRowHeight}
                emptyState={emptyStateToDisplay}
              />
            )
          }
          getId={getId}
          hasLoadingCompleted={hasLoadingCompleted}
          {...(isRowSelected === undefined ? {} : { isRowSelected })}
          {...(onRowClick === undefined ? {} : { onRowClick })}
          placeholderRowCount={
            hasLoadingCompleted ? placeholderRowCount : rowsPerPage
          }
          desktopBreakpoint={desktopBreakpoint}
        />
        {hasLoadingCompleted && totalCount > 0 ? (
          <TablePagination
            rowsPerPageOptions={[rowsPerPage]}
            component="div"
            count={totalCount}
            rowsPerPage={rowsPerPage}
            page={paginationIndex}
            onPageChange={(_, newPage) => {
              updateParams((params) => {
                params.set(pageParamName, (newPage + 1).toString());
              });
            }}
          />
        ) : null}
      </Paper>
    </Frame>
  );
};

export default ListFrame;
export type { EmptyStateDefinition };
