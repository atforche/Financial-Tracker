import { type JSX, useEffect, useState } from "react";
import Toast from "@/framework/alerts/Toast";
import { Typography } from "@mui/material";

/**
 * Props for the ErrorAlert component.
 */
interface ErrorAlertProps {
  readonly errorMessage: string | null;
  readonly unmappedErrors: string | null;
}

/**
 * Component that displays an error alert for the given API error.
 */
const ErrorAlert = function ({
  errorMessage,
  unmappedErrors,
}: ErrorAlertProps): JSX.Element {
  const [dismissedError, setDismissedError] = useState<ErrorAlertProps | null>(
    null,
  );

  useEffect(() => {
    if (errorMessage === null) {
      setDismissedError(null);
    }
  }, [errorMessage]);

  const open =
    errorMessage !== null &&
    (errorMessage !== dismissedError?.errorMessage ||
      unmappedErrors !== dismissedError.unmappedErrors);

  return (
    <Toast
      severity="error"
      open={open}
      onClose={() => {
        setDismissedError({ errorMessage, unmappedErrors });
      }}
    >
      {errorMessage !== null && (
        <>
          <Typography variant="body2">{errorMessage}</Typography>
          {unmappedErrors !== null && (
            <Typography variant="caption" sx={{ whiteSpace: "pre-line" }}>
              {unmappedErrors}
            </Typography>
          )}
        </>
      )}
    </Toast>
  );
};

export default ErrorAlert;
