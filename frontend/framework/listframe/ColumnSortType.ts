/**
 * Enum representing the possible sorting states for a column in a list frame.
 */
enum ColumnSortType {
  Ascending = "Ascending",
  Descending = "Descending",
}

/**
 * Gets the sort a column moves to when its header is clicked: ascending, then descending, then unsorted.
 */
const getNextSortType = function (
  sortType: ColumnSortType | null,
): ColumnSortType | null {
  if (sortType === null) {
    return ColumnSortType.Ascending;
  }
  return sortType === ColumnSortType.Ascending
    ? ColumnSortType.Descending
    : null;
};

export { getNextSortType };
export default ColumnSortType;
