import {
  DialogActions,
  DialogContent,
  DialogTitle,
  Dialog as MuiDialog,
  Stack,
} from "@mui/material";
import type { JSX } from "react/jsx-runtime";

/**
 * Props for the Dialog component.
 * @param title - Title to display along the top of the dialog.
 * @param content - Content to display within the dialog.
 * @param actions - Actions to display along the bottom of the dialog.
 */
interface DialogProps {
  readonly title: string;
  readonly content: JSX.Element;
  readonly actions: JSX.Element;
}

/**
 * Component that presents the user with a dialog.
 * @param props - Props for the Dialog component.
 * @returns JSX element representing the Dialog.
 */
const Dialog = function ({
  title,
  content,
  actions,
}: DialogProps): JSX.Element {
  return (
    <MuiDialog open maxWidth="lg">
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        {title}
      </DialogTitle>
      <DialogContent>
        <Stack
          direction="column"
          spacing={3}
          sx={{ paddingLeft: "25px", paddingRight: "25px", paddingTop: "25px" }}
        >
          {content}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Stack direction="row" justifyContent="right">
          {actions}
        </Stack>
      </DialogActions>
    </MuiDialog>
  );
};

export default Dialog;
