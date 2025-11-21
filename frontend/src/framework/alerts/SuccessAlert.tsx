import type { JSX } from "react";
import Toast from "@framework/alerts/Toast";

/**
 * Props for the SuccessAlert component.
 * @param {string | null} message - The success message to display.
 */
interface SuccessAlertProps {
  readonly message: string | null;
}

/**
 * Component that displays an success message.
 * @param {SuccessAlertProps} props - Props for the SuccessAlert component.
 * @returns {JSX.Element} JSX element representing the SuccessAlert.
 */
const SuccessAlert = function ({ message }: SuccessAlertProps): JSX.Element {
  return <Toast message={message} severity="success" autoHideDuration={5000} />;
};

export default SuccessAlert;
