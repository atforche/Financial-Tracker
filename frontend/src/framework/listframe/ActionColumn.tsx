import type Action from "@framework/listframe/Action";
import Column from "@framework/listframe/Column";
import type HeaderAction from "@framework/listframe/HeaderAction";

/**
 * Class that represents a column that displays action buttons in a list frame.
 */
class ActionColumn<T> extends Column<T> {
  /**
   * Constructs a new instance of this class.
   * @param {HeaderAction?} headerAction - Optional action that appears in the header of the column.
   * @param {Action<T>[]} actions - List of actions to be displayed in the column.
   */
  public constructor(headerAction: HeaderAction | null, actions: Action<T>[]) {
    super({
      id: "actions",
      label: headerAction ? headerAction.getElement() : "",
      mapping: (value: T) => (
        <>{actions.map((action) => action.getElement(value))}</>
      ),
      align: "right",
      width: 125,
    });
  }
}

export default ActionColumn;
