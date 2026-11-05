import { Stack, Typography } from "@mui/material";
import type { JSX } from "react";

/**
 * Displays one labeled figure behind a goal's result.
 */
const GoalDetailRow = function ({
  label,
  value,
  valueInset = 0,
}: {
  readonly label: string;
  readonly value: string;
  readonly valueInset?: number;
}): JSX.Element {
  return (
    <Stack
      direction="row"
      spacing={2}
      sx={{ alignItems: "baseline", justifyContent: "space-between" }}
    >
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography
        variant="body2"
        sx={{ fontWeight: 600, pr: `${valueInset.toString()}px` }}
      >
        {value}
      </Typography>
    </Stack>
  );
};

export default GoalDetailRow;
