import { Alert, type AlertProps, Snackbar } from "@mui/material";
import type { JSX, ReactNode } from "react";

/**
 * Props for the Toast component.
 */
interface ToastProps {
  readonly children: ReactNode;
  readonly severity: AlertProps["severity"];
  readonly open: boolean;
  readonly onClose: () => void;
  readonly autoHideDuration?: number | null;
}

/**
 * Component that displays a toast message to the user.
 */
const Toast = function ({
  children,
  severity,
  open,
  onClose,
  autoHideDuration = null,
}: ToastProps): JSX.Element {
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
        {children}
      </Alert>
    </Snackbar>
  );
};

export default Toast;
