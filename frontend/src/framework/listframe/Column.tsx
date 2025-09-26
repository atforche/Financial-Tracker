/**
 * Column interface represents a column in a list frame.
 */
interface Column<T> {
  /**
   * Get the header element for this column.
   * @returns {JSX.Element} The header element for this column.
   */
  getHeaderElement: () => JSX.Element;

  /**
   * Get the row element for this column.
   * @param {T} value - The object to map to a value for this column.
   * @returns {JSX.Element} The row element for this column.
   */
  getRowElement: (value: T) => JSX.Element;
}

export default Column;
