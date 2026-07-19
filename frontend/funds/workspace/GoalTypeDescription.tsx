import { Alert, AlertTitle } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the GoalTypeDescription component.
 */
interface GoalTypeDescriptionProps {
  readonly title: string;
  readonly description: string;
}

/**
 * Displays an explanation of the selected goal type.
 */
const GoalTypeDescription = function ({
  title,
  description,
}: GoalTypeDescriptionProps): JSX.Element {
  return (
    <Alert severity="info" variant="outlined" icon={false}>
      <AlertTitle>{title}</AlertTitle>
      {description}
    </Alert>
  );
};

export default GoalTypeDescription;
