import { IconButton } from "@mui/material";

/**
 * Class that represents an action that can be performed on an entry in a list frame.
 */
class Action<T> {
  private readonly _icon: JSX.Element;
  private readonly _getOnClickCallback: (row: T) => () => void;

  /**
   * Constructs a new instance of this class.
   * @param {JSX.Element} icon - The icon to display for this action.
   * @param {Function} getOnClickCallback - A function that returns the onClick handler for the action button.
   */
  public constructor(
    icon: JSX.Element,
    getOnClickCallback: (row: T) => () => void,
  ) {
    this._icon = icon;
    this._getOnClickCallback = getOnClickCallback;
  }

  /**
   * Returns the JSX element representing this action.
   * @param {T} row - The data row associated with this action.
   * @returns {JSX.Element} The JSX element representing this action.
   */
  public getElement(row: T): JSX.Element {
    return (
      <IconButton onClick={this._getOnClickCallback(row)}>
        {this._icon}
      </IconButton>
    );
  }
}

export default Action;
