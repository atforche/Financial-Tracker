import type Column from "@framework/listframe/Column";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";

/**
 * Class that represents a column that displays string values in a list frame.
 */

/**
 * Class that represents a column that displays string values in a list frame.
 */
class StringColumn<T> implements Column<T> {
  private readonly label: string;
  private readonly mapping: (value: T) => string;

  /**
   * Constructs a new instance of this class.
   * @param {string} label - Label for this column.
   * @param {Function} mapping - Mapping function that maps an object to the value that should be displayed in this column.
   */
  public constructor(label: string, mapping: (value: T) => string) {
    this.label = label;
    this.mapping = mapping;
  }

  /**
   * Get the header element for this column.
   * @returns {JSX.Element} The header element for this column.
   */
  public getHeaderElement(): JSX.Element {
    return (
      <ColumnHeader
        id={this.label}
        label={this.label}
        align="left"
        width={170}
      />
    );
  }

  /**
   * Get the row element for this column.
   * @param {T} value - The object to map to a value for this column.
   * @returns {JSX.Element} The row element for this column.
   */
  public getRowElement(value: T): JSX.Element {
    return (
      <ColumnCell
        id={this.label}
        value={this.mapping(value)}
        align="left"
        width={170}
      />
    );
  }
}

export default StringColumn;
