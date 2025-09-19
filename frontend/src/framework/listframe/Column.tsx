import TableCell from "@mui/material/TableCell";

/**
 * Interface representing the arguments for the Column constructor.
 * @param {string} id - Id that uniquely identifies this Column.
 * @param {string | JSX.Element} label - Label for this Column.
 * @param {Function} mapping - Mapping function that maps an object to the value that should be displayed in this column.
 * @param {"center" | "left"} align - Alignment for this column.
 * @param {number} width - Width for this column.
 */
interface ColumnArgs<T> {
  id: string;
  label: string | JSX.Element;
  mapping: (value: T) => string | JSX.Element;
  align: "center" | "left" | "right";
  width: number;
}

/**
 * Column class represents a column in a list frame.
 */
class Column<T> {
  private readonly _id: string;
  private readonly _label: string | JSX.Element;
  private readonly _mapping: (value: T) => string | JSX.Element;
  private readonly _align: "center" | "left" | "right";
  private readonly _width: number;

  /**
   * Constructor for the Column class.
   * @param {ColumnArgs<T>} args - Arguments for the Column constructor.
   */
  protected constructor({ id, label, mapping, align, width }: ColumnArgs<T>) {
    this._id = id;
    this._label = label;
    this._mapping = mapping;
    this._align = align;
    this._width = width;
  }

  /**
   * Get the header element for this column.
   * @returns {JSX.Element} The header element for this column.
   */
  public getHeaderElement(): JSX.Element {
    return (
      <TableCell
        key={this._id}
        align={this._align}
        style={{ minWidth: this._width }}
        sx={{ backgroundColor: "primary.main", color: "white" }}
      >
        {this._label}
      </TableCell>
    );
  }

  /**
   * Get the row element for this column.
   * @param {T} value - The object to map to a value for this column.
   * @returns {JSX.Element} The row element for this column.
   */
  public getRowElement(value: T): JSX.Element {
    return (
      <TableCell
        key={this._id}
        align={this._align}
        style={{ minWidth: this._width }}
      >
        {this._mapping(value)}
      </TableCell>
    );
  }
}

export default Column;
