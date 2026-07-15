import ColumnSortType from "@/framework/listframe/ColumnSortType";

/**
 * Props for a column sorting helper.
 */
interface ColumnSortProps {
  readonly sortType: ColumnSortType | null;
  readonly onSort: (sortType: ColumnSortType | null) => void;
}

/**
 * Creates a column sorting helper bound to the current sort and its setter.
 * @param currentSort - The currently selected API sort value.
 * @param setSort - Sets the selected API sort value.
 * @returns A function that creates sorting props for a column.
 */
const createColumnSortProps = function <TSort>(
  currentSort: TSort | null | undefined,
  setSort: (sort: TSort | null) => void,
): (ascendingSort: TSort, descendingSort: TSort) => ColumnSortProps {
  return (ascendingSort, descendingSort) => ({
    sortType:
      currentSort === ascendingSort
        ? ColumnSortType.Ascending
        : currentSort === descendingSort
          ? ColumnSortType.Descending
          : null,
    onSort: (sortType): void => {
      setSort(
        sortType === ColumnSortType.Ascending
          ? ascendingSort
          : sortType === ColumnSortType.Descending
            ? descendingSort
            : null,
      );
    },
  });
};

export default createColumnSortProps;
