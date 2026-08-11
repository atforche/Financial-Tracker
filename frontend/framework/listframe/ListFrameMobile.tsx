import type { JSX, ReactNode } from "react";
import { Paper, Stack } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ListFrameMobileRow from "@/framework/listframe/ListFrameMobileRow";

/**
 * Props for the ListFrameMobile component.
 */
interface ListFrameMobileProps<T> {
  readonly columns: readonly ColumnDefinition<T>[];
  readonly data: readonly T[] | null;
  readonly emptyState: ReactNode;
  readonly getId: (item: T) => string;
  readonly hasLoadingCompleted: boolean;
  readonly isRowSelected?: (item: T) => boolean;
  readonly onRowClick?: (item: T) => void;
  readonly placeholderRowCount: number;
  readonly desktopBreakpoint: "sm" | "md" | "lg" | "xl";
}

/**
 * Displays list-frame data as cards on narrow screens.
 */
const ListFrameMobile = function <T>({
  columns,
  data,
  emptyState,
  getId,
  hasLoadingCompleted,
  isRowSelected,
  onRowClick,
  placeholderRowCount,
  desktopBreakpoint,
}: ListFrameMobileProps<T>): JSX.Element {
  const items = hasLoadingCompleted ? (data ?? []) : [];

  return (
    <Stack
      spacing={1.25}
      sx={{
        display: { xs: "flex", [desktopBreakpoint]: "none" },
        p: 1.5,
      }}
    >
      {items.map((item) => (
        <ListFrameMobileRow
          key={getId(item)}
          columns={columns}
          getId={getId}
          item={item}
          {...(isRowSelected === undefined ? {} : { isRowSelected })}
          {...(onRowClick === undefined ? {} : { onRowClick })}
        />
      ))}
      {placeholderRowCount > 0
        ? Array(placeholderRowCount)
            .fill(null)
            .map((_, index) => (
              <Paper
                key={`mobile-skeleton-${index}`}
                variant="outlined"
                sx={{ borderRadius: 2, height: 96 }}
              />
            ))
        : null}
      {emptyState}
    </Stack>
  );
};

export default ListFrameMobile;
