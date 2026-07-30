import { Alert, AlertTitle } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the FundGoalTypeDescription component.
 */
interface FundGoalTypeDescriptionProps {
  readonly title: string;
  readonly description: string;
}

/**
 * Displays an explanation of the selected Fund Goal type.
 */
const FundGoalTypeDescription = function ({
  title,
  description,
}: FundGoalTypeDescriptionProps): JSX.Element {
  return (
    <Alert severity="info" variant="outlined" icon={false}>
      <AlertTitle>{title}</AlertTitle>
      {description}
    </Alert>
  );
};

export default FundGoalTypeDescription;
