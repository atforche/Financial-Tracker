import Column from "@ui/framework/listframe/Column";

/**
 * Class that represents a column that displays string values in a list frame.
 */
class StringColumn<T> extends Column<T> {
  /**
   * Constructs a new instance of this class.
   * @param {string} label - Label for this column.
   * @param {Function} mapping - Mapping function that maps an object to the value that should be displayed in this column.
   */
  public constructor(label: string, mapping: (value: T) => string) {
    super({
      id: label,
      label,
      mapping,
      align: "left",
      width: 170,
    });
  }
}

export default StringColumn;
