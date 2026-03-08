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
 */
interface DialogProps {
  readonly title: string;
  readonly content: JSX.Element;
  readonly actions: JSX.Element;
  readonly onClose: () => void;
  readonly headerActions?: JSX.Element | null;
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
  onClose,
  headerActions = null,
}: DialogProps): JSX.Element {
  return (
    <MuiDialog open maxWidth="lg" onClose={onClose}>
      <DialogTitle
        sx={{
          backgroundColor: "primary.main",
          color: "white",
          minWidth: "500px",
        }}
      >
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
        >
          {title}
          {headerActions}
        </Stack>
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
