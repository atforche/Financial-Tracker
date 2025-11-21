import type { ApiError } from "@data/ApiError";
import type { JSX } from "react";
import Toast from "@framework/alerts/Toast";

/**
 * Props for the ErrorAlert component.
 * @param {ApiError | null} error - The API error to display.
 */
interface ErrorAlertProps {
  readonly error: ApiError | null;
}

/**
 * Component that displays an error alert for the given API error.
 * @param {ErrorAlertProps} props - Props for the ErrorAlert component.
 * @returns {JSX.Element} JSX element representing the ErrorAlert.
 */
const ErrorAlert = function ({ error }: ErrorAlertProps): JSX.Element {
  return (
    <Toast
      message={error?.message ?? null}
      severity="error"
      autoHideDuration={null}
    />
  );
};

export default ErrorAlert;
