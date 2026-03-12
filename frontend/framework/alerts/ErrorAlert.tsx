import type { JSX } from "react";
import Toast from "@/framework/alerts/Toast";
import { Typography } from "@mui/material";

/**
 * Props for the ErrorAlert component.
 */
interface ErrorAlertProps {
  readonly errorMessage: string | null;
}

/**
 * Component that displays an error alert for the given API error.
 */
const ErrorAlert = function ({ errorMessage }: ErrorAlertProps): JSX.Element {
  let content: JSX.Element | null = null;
  if (errorMessage !== null) {
    content = <Typography variant="body2">{errorMessage}</Typography>;
  }
  return (
    <Toast severity="error" autoHideDuration={null}>
      {content}
    </Toast>
  );
};

export default ErrorAlert;
