import type { JSX, ReactNode } from "react";
import { Stack, Typography } from "@mui/material";

/**
 * Props for the PayrollSectionHeading component.
 */
interface PayrollSectionHeadingProps {
  readonly title: string;
  readonly description: string;
  readonly action?: ReactNode;
}

/**
 * Displays the heading and explanatory text for a payroll section.
 */
const PayrollSectionHeading = function ({
  title,
  description,
  action = null,
}: PayrollSectionHeadingProps): JSX.Element {
  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      spacing={1.5}
      justifyContent="space-between"
      alignItems={{ xs: "stretch", sm: "flex-start" }}
    >
      <Stack spacing={0.25}>
        <Typography variant="subtitle2">{title}</Typography>
        <Typography variant="body2" color="text.secondary">
          {description}
        </Typography>
      </Stack>
      {action}
    </Stack>
  );
};

export default PayrollSectionHeading;
