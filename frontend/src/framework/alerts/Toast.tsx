import { Alert, Snackbar, type SnackbarCloseReason } from "@mui/material";
import { type JSX, useCallback, useState } from "react";

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

  const handleAlertClose = useCallback<
    (event: React.SyntheticEvent | Event) => void
  >(() => {
    setOpen(false);
    setHasBeenCleared(true);
  }, []);

  const handleSnackbarClose = useCallback<
    (event: React.SyntheticEvent | Event, reason: SnackbarCloseReason) => void
  >(
    (event, reason) => {
      if (reason === "clickaway") {
        return;
      }
      handleAlertClose(event);
    },
    [handleAlertClose],
  );

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
      onClose={handleSnackbarClose}
      autoHideDuration={autoHideDuration}
    >
      <Alert
        severity={severity}
        variant="filled"
        sx={{ width: "100%" }}
        onClose={handleAlertClose}
      >
        {message}
      </Alert>
    </Snackbar>
  );
};

export default Toast;
