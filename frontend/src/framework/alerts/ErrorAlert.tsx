import type ApiErrorHandler from "@data/ApiErrorHandler";
import ErrorHelperText from "@framework/dialog/ErrorHelperText";
import type { JSX } from "react";
import Toast from "@framework/alerts/Toast";
import { Typography } from "@mui/material";

/**
 * Props for the ErrorAlert component.
 */
interface ErrorAlertProps {
  readonly errorHandler: ApiErrorHandler | null;
}

/**
 * Component that displays an error alert for the given API error.
 * @param props - Props for the ErrorAlert component.
 * @returns JSX element representing the ErrorAlert.
 */
const ErrorAlert = function ({ errorHandler }: ErrorAlertProps): JSX.Element {
  let content: JSX.Element | null = null;
  const errorDescription = errorHandler?.getErrorDescription() ?? null;
  if (errorDescription !== null) {
    content = (
      <>
        <Typography variant="body2">{errorDescription}</Typography>
        <ErrorHelperText errorHandler={errorHandler} />
      </>
    );
  }
  return (
    <Toast severity="error" autoHideDuration={null}>
      {content}
    </Toast>
  );
};

export default ErrorAlert;
