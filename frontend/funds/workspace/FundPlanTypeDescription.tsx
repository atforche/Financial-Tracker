import { Alert, AlertTitle } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the FundPlanTypeDescription component.
 */
interface FundPlanTypeDescriptionProps {
  readonly title: string;
  readonly description: string;
}

/**
 * Displays an explanation of the selected Funding Plan type.
 */
const FundPlanTypeDescription = function ({
  title,
  description,
}: FundPlanTypeDescriptionProps): JSX.Element {
  return (
    <Alert severity="info" variant="outlined" icon={false}>
      <AlertTitle>{title}</AlertTitle>
      {description}
    </Alert>
  );
};

export default FundPlanTypeDescription;
