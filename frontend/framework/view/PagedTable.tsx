"use client";

import {
  Box,
  ButtonBase,
  Stack,
  TablePagination,
  TableSortLabel,
  Typography,
} from "@mui/material";
import { type JSX, type ReactNode, useEffect } from "react";
import { getPaginationIndex, getRowsPerPage } from "@/framework/view/pagination";
import ChevronRight from "@mui/icons-material/ChevronRight";
import ContentSurface from "@/framework/view/ContentSurface";
import Link from "next/link";
import ResponsivePageSize from "@/framework/view/ResponsivePageSize";
import type { Route } from "next";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * One column of a PagedTable.
 */
interface PagedTableColumn<T> {
  readonly name: string;
  readonly header: string;
  readonly getValue: (item: T) => ReactNode;

  /**
   * CSS grid track size for the column.
   */
  readonly width: string;
  readonly alignment?: "left" | "right";

  /**
   * Hides the column on phone-width screens.
   */
  readonly hideOnMobile?: boolean;

  /**
   * The server's ascending and descending sorts for the column.
   */
  readonly sort?: readonly [string, string];

  /**
   * Sorts descending on the first click, for columns where the largest values matter most.
   */
  readonly descendingFirst?: boolean;
}

/**
 * Message and action shown when the table has no rows.
 */
interface PagedTableEmptyState {
  readonly title: string;
  readonly description: string;
  readonly action?: ReactNode;
}

/**
 * Props for the PagedTable component.
 */
interface PagedTableProps<T> {
  readonly title: string;
  readonly headerContent?: ReactNode;
  readonly columns: readonly PagedTableColumn<T>[];
  readonly data: readonly T[];
  readonly getKey: (item: T) => string;
  readonly totalCount: number;

  /**
   * Where each row links to. Rows that open something on the same page use onRowClick instead.
   */
  readonly getHref?: (item: T) => Route;
  readonly onRowClick?: (item: T) => void;

  /**
   * The search parameter holding the sort, needed only when a column sorts.
   */
  readonly sortParamName?: string;

  /**
   * The sort the server applies while the URL names none.
   */
  readonly defaultSort?: string;
  readonly pageParamName: string;
  readonly emptyState: PagedTableEmptyState;
}

/**
 * Gets the grid tracks for the visible columns, with a final slot for the row chevron.
 */
const getGridColumns = function <T>(
  columns: readonly PagedTableColumn<T>[],
  includeMobileHidden: boolean,
): string {
  return [
    ...columns
      .filter((column) => includeMobileHidden || column.hideOnMobile !== true)
      .map((column) => column.width),
    "1.25rem",
  ].join(" ");
};

/**
 * Lists a server-sorted, server-paged collection as linked rows on a surface, under sortable column headers. The sort
 * and page are kept in the URL.
 */
const PagedTable = function <T>({
  title,
  headerContent,
  columns,
  data,
  getKey,
  getHref,
  onRowClick,
  totalCount,
  sortParamName = "sort",
  defaultSort,
  pageParamName,
  emptyState,
}: PagedTableProps<T>): JSX.Element {
  const searchParams = useSearchParams();
  // A new sort starts from the first page
  const updateSortParams = useSearchParamUpdater([pageParamName]);
  const updatePageParams = useSearchParamUpdater([]);
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const currentSort = searchParams.get(sortParamName) ?? defaultSort ?? null;
  const currentPage = searchParams.get(pageParamName);
  const paginationIndex = getPaginationIndex(
    currentPage,
    totalCount,
    rowsPerPage,
  );

  // A page past the end, such as after a larger page size, moves to the last page
  useEffect(() => {
    const canonicalPage = (paginationIndex + 1).toString();
    if (currentPage !== null && currentPage !== canonicalPage) {
      updatePageParams((params) => {
        params.set(pageParamName, canonicalPage);
      });
    }
  }, [currentPage, pageParamName, paginationIndex, updatePageParams]);
  const rowSx = {
    display: "grid",
    alignItems: "center",
    columnGap: 2,
    gridTemplateColumns: {
      xs: getGridColumns(columns, false),
      md: getGridColumns(columns, true),
    },
    px: 1,
  };
  const getCellSx = function (
    column: PagedTableColumn<T>,
  ): Record<string, unknown> {
    return {
      display: {
        xs: column.hideOnMobile === true ? "none" : "flex",
        md: "flex",
      },
      justifyContent: column.alignment === "right" ? "flex-end" : "flex-start",
      minWidth: 0,
    };
  };

  return (
    <ContentSurface>
      <ResponsivePageSize desktopBreakpoint="md" />
      <Stack spacing={1.5}>
        <Stack
          direction="row"
          spacing={2}
          sx={{ alignItems: "center", justifyContent: "space-between" }}
        >
          <Typography variant="h5">{title}</Typography>
          {headerContent}
        </Stack>
        {totalCount === 0 ? (
          <Stack spacing={1} sx={{ alignItems: "center", py: 4 }}>
            <Typography variant="subtitle1">{emptyState.title}</Typography>
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ textAlign: "center" }}
            >
              {emptyState.description}
            </Typography>
            {emptyState.action === undefined ? null : (
              <Box sx={{ pt: 1 }}>{emptyState.action}</Box>
            )}
          </Stack>
        ) : (
          <Box>
            <Box
              sx={{
                ...rowSx,
                pb: 0.75,
                borderBottom: "1px solid",
                borderColor: "divider",
              }}
            >
              {columns.map((column) => {
                const [ascending, descending] = column.sort ?? [];
                const isActive =
                  currentSort === ascending || currentSort === descending;
                return (
                  <Box key={column.name} sx={getCellSx(column)}>
                    {column.sort === undefined ? (
                      <Typography variant="caption" color="text.secondary">
                        {column.header}
                      </Typography>
                    ) : (
                      <TableSortLabel
                        active={isActive}
                        direction={currentSort === descending ? "desc" : "asc"}
                        onClick={() => {
                          updateSortParams((params) => {
                            // An active column flips direction; another starts in its first direction
                            let next = ascending;
                            if (currentSort === ascending) {
                              next = descending;
                            } else if (
                              currentSort !== descending &&
                              column.descendingFirst === true
                            ) {
                              next = descending;
                            }
                            params.set(sortParamName, next ?? "");
                          });
                        }}
                        sx={{
                          typography: "caption",
                          color: "text.secondary",
                          flexDirection:
                            column.alignment === "right"
                              ? "row-reverse"
                              : "row",
                        }}
                      >
                        {column.header}
                      </TableSortLabel>
                    )}
                  </Box>
                );
              })}
            </Box>
            {data.map((item, index) => (
              <ButtonBase
                key={getKey(item)}
                {...(getHref === undefined
                  ? {
                      onClick: (): void => {
                        onRowClick?.(item);
                      },
                    }
                  : { component: Link, href: getHref(item) })}
                sx={{
                  ...rowSx,
                  width: "100%",
                  py: 1,
                  borderTop: index === 0 ? "none" : "1px solid",
                  borderColor: "divider",
                  textAlign: "left",
                  "&:hover": { bgcolor: "action.hover" },
                }}
              >
                {columns.map((column) => (
                  <Typography
                    key={column.name}
                    variant="body2"
                    component="div"
                    noWrap
                    sx={getCellSx(column)}
                  >
                    {column.getValue(item)}
                  </Typography>
                ))}
                <ChevronRight fontSize="small" color="action" />
              </ButtonBase>
            ))}
          </Box>
        )}
        {totalCount > rowsPerPage ? (
          <TablePagination
            component="div"
            rowsPerPageOptions={[rowsPerPage]}
            count={totalCount}
            rowsPerPage={rowsPerPage}
            page={paginationIndex}
            onPageChange={(_, newPage) => {
              updatePageParams((params) => {
                params.set(pageParamName, (newPage + 1).toString());
              });
            }}
          />
        ) : null}
      </Stack>
    </ContentSurface>
  );
};

export type { PagedTableColumn };
export default PagedTable;
