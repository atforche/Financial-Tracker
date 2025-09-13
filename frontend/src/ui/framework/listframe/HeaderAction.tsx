import { Box, Button } from "@mui/material";

/**
 * Class that represents an action that appears in the header of a list frame.
 */
class HeaderAction {
  private readonly _icon: JSX.Element;
  private readonly _caption: string;
  private readonly _onClick: () => void;

  /**
   * Constructs a new instance of this class.
   * @param {JSX.Element} icon - The icon to display for this action.
   * @param {string} caption - The caption to display for this action.
   * @param {Function} onClick - The onClick handler for the action button.
   */
  public constructor(icon: JSX.Element, caption: string, onClick: () => void) {
    this._icon = icon;
    this._caption = caption;
    this._onClick = onClick;
  }

  /**
   * Returns the JSX element representing this action.
   * @returns {JSX.Element} The JSX element representing this action.
   */
  public getElement(): JSX.Element {
    return (
      <Box sx={{ verticalAlign: "middle" }}>
        <Button
          variant="contained"
          startIcon={this._icon}
          disableElevation
          sx={{ backgroundColor: "primary", border: 1, borderColor: "white" }}
          onClick={this._onClick}
        >
          {this._caption}
        </Button>
      </Box>
    );
  }
}

export default HeaderAction;
