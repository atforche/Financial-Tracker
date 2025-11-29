import type { ApiError, ApiErrorDetail } from "@data/ApiError";
import type { JSX } from "react";
import Toast from "@framework/alerts/Toast";
import { Typography } from "@mui/material";

/**
 * Props for the ErrorAlert component.
 * @param error - The API error to display.
 * @param detailFilter - Optional filter function to select specific error details to display.
 */
interface ErrorAlertProps {
  readonly error: ApiError | null;
  readonly detailFilter?: ((detail: ApiErrorDetail) => boolean) | null;
}

/**
 * Component that displays an error alert for the given API error.
 * @param props - Props for the ErrorAlert component.
 * @returns JSX element representing the ErrorAlert.
 */
const ErrorAlert = function ({
  error,
  detailFilter = null,
}: ErrorAlertProps): JSX.Element {
  let content: JSX.Element | null = null;
  if (error !== null) {
    content = <Typography variant="body2">{error.message}</Typography>;
  }
  const details =
    (detailFilter === null
      ? error?.details
      : error?.details.filter(detailFilter)) ?? [];
  if (details.length > 0) {
    content = (
      <>
        {content}
        {details.map((detail) => (
          <Typography variant="caption" key={detail.description}>
            - {detail.description}
          </Typography>
        ))}
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
