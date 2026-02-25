import type ApiErrorHandler from "@data/ApiErrorHandler";
import type { JSX } from "react";
import { Typography } from "@mui/material";

/**
 * Props for the ErrorHelperText component.
 */
interface ErrorHelperTextProps {
  readonly errorHandler: ApiErrorHandler | null;
  readonly errorKey?: string | null;
}

/**
 * Component that displays helper text for the given API error details.
 * @param props - Props for the ErrorHelperText component.
 * @returns JSX element representing the ErrorHelperText.
 */
const ErrorHelperText = function ({
  errorHandler,
  errorKey = null,
}: ErrorHelperTextProps): JSX.Element | null {

  const error = errorHandler?.handleError(errorKey) ?? null;
  if (error === null) {
    return null;
  }
  return (
    <>
      {error.map((detail) => (
        <Typography variant="caption" key={detail}>
          — {detail}
          <br />
        </Typography>
      ))}
    </>
  );
};

export default ErrorHelperText;
