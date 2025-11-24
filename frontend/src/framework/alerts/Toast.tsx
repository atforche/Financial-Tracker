import { Alert, Snackbar } from "@mui/material";
import { type JSX, useState } from "react";

/**
 * Props for the Toast component.
 * @param {string} message - The message to display in the toast.
 * @param { "error" | "success" } severity - The severity level of the toast.
 * @param {number | undefined} autoHideDuration - Duration in milliseconds before the toast auto-hides.
 */
interface ToastProps {
  readonly message: string | null;
  readonly severity: "error" | "success";
  readonly autoHideDuration: number | null;
}

/**
 * Component that displays an toast message to the user.
 * @param {Toast} props - Props for the Toast component.
 * @returns {JSX.Element} JSX element representing the Toast.
 */
const Toast = function ({
  message,
  severity,
  autoHideDuration,
}: ToastProps): JSX.Element {
  const [open, setOpen] = useState(false);
  const [hasBeenCleared, setHasBeenCleared] = useState(false);
  const onClose = function (): void {
    setOpen(false);
    setHasBeenCleared(true);
  };
  if (message !== null && !open && !hasBeenCleared) {
    setOpen(true);
  } else if (message === null && (open || hasBeenCleared)) {
    setOpen(false);
    setHasBeenCleared(false);
  }
  return (
    <Snackbar
      open={open}
      anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
      onClose={(_, reason) => {
        if (reason === "clickaway") {
          return;
        }
        onClose();
      }}
      autoHideDuration={autoHideDuration}
    >
      <Alert
        severity={severity}
        variant="filled"
        sx={{ width: "100%" }}
        onClose={onClose}
      >
        {message}
      </Alert>
    </Snackbar>
  );
};

export default Toast;
