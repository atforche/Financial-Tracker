import { Box, Checkbox, FormControlLabel } from "@mui/material";
import type { JSX } from "react";

/**
 * Props for the PayrollTaxTreatmentSelector component.
 */
interface PayrollTaxTreatmentSelectorProps {
  readonly value: number;
  readonly setValue: ((value: number) => void) | null;
  readonly labelPrefix?: string;
}

const taxTreatments = [
  [1, "Federal income"],
  [2, "Social Security"],
  [4, "Medicare"],
  [8, "State income"],
] as const;

const hasTaxTreatment = (value: number, treatment: number): boolean =>
  Math.floor(value / treatment) % 2 === 1;

/**
 * Selects the wage bases associated with a payroll item.
 */
const PayrollTaxTreatmentSelector = function ({
  value,
  setValue,
  labelPrefix = "",
}: PayrollTaxTreatmentSelectorProps): JSX.Element {
  return (
    <Box>
      {taxTreatments.map(([treatment, label]) => (
        <FormControlLabel
          key={treatment}
          label={`${labelPrefix}${label}`}
          control={
            <Checkbox
              disabled={setValue === null}
              checked={hasTaxTreatment(value, treatment)}
              onChange={(_, selected): void => {
                if (setValue === null) {
                  return;
                }
                const currentlySelected = hasTaxTreatment(value, treatment);
                setValue(
                  selected === currentlySelected
                    ? value
                    : selected
                      ? value + treatment
                      : value - treatment,
                );
              }}
            />
          }
        />
      ))}
    </Box>
  );
};

export default PayrollTaxTreatmentSelector;
