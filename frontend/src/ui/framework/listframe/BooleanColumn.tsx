import { Checkbox } from "@mui/material";
import Column from "@ui/framework/listframe/Column";

/**
 * Class that represents a column that displays boolean values in a list frame.
 */
class BooleanColumn<T> extends Column<T> {
  /**
   * Constructs a new instance of this class.
   * @param {string} label - Label for this column.
   * @param {Function} mapping - Mapping function that maps an object to the value that should be displayed in this column.
   */
  public constructor(label: string, mapping: (value: T) => boolean) {
    super({
      id: label,
      label,
      mapping: (value) =>
        mapping(value) ? <Checkbox disabled checked /> : <Checkbox disabled />,
      align: "center",
      width: 100,
    });
  }
}

export default BooleanColumn;
