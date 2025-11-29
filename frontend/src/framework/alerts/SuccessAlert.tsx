import type { JSX } from "react";
import Toast from "@framework/alerts/Toast";

/**
 * Props for the SuccessAlert component.
 * @param message - The success message to display.
 */
interface SuccessAlertProps {
  readonly message: string | null;
}

/**
 * Component that displays an success message.
 * @param props - Props for the SuccessAlert component.
 * @returns JSX element representing the SuccessAlert.
 */
const SuccessAlert = function ({ message }: SuccessAlertProps): JSX.Element {
  return (
    <Toast severity="success" autoHideDuration={5000}>
      {message}
    </Toast>
  );
};

export default SuccessAlert;
