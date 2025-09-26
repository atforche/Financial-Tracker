import type Action from "@framework/listframe/Action";
import type Column from "@framework/listframe/Column";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import type HeaderAction from "@framework/listframe/HeaderAction";

/**
 * Class that represents a column that displays action buttons in a list frame.
 */
class ActionColumn<T> implements Column<T> {
  private readonly headerAction: HeaderAction | null;
  private readonly actions: Action<T>[];

  /**
   * Constructs a new instance of this class.
   * @param {HeaderAction?} headerAction - Optional action that appears in the header of the column.
   * @param {Action<T>[]} actions - List of actions to be displayed in the column.
   */
  public constructor(headerAction: HeaderAction | null, actions: Action<T>[]) {
    this.headerAction = headerAction;
    this.actions = actions;
  }

  /**
   * Get the header element for this column.
   * @returns {JSX.Element} The header element for this column.
   */
  public getHeaderElement(): JSX.Element {
    return (
      <ColumnHeader
        id="actions"
        label={this.headerAction ? this.headerAction.getElement() : ""}
        align="right"
        width={125}
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
        id="actions"
        value={<>{this.actions.map((action) => action.getElement(value))}</>}
        align="right"
        width={125}
      />
    );
  }
}

export default ActionColumn;
