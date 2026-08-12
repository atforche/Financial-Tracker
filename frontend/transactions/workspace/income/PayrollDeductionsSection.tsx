/* eslint-disable @typescript-eslint/explicit-function-return-type -- JSX callbacks inherit the shared field callback signatures. */
import { Box, MenuItem, Stack, TextField } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { IncomeDeductionDraft } from "@/transactions/workspace/income/helpers";
import type { JSX } from "react";
import PayrollAddButton from "@/transactions/workspace/income/PayrollAddButton";
import PayrollEmptyState from "@/transactions/workspace/income/PayrollEmptyState";
import PayrollItemShell from "@/transactions/workspace/income/PayrollItemShell";
import PayrollSectionHeading from "@/transactions/workspace/income/PayrollSectionHeading";
import PayrollTaxTreatmentSelector from "@/transactions/workspace/income/PayrollTaxTreatmentSelector";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the PayrollDeductionsSection component.
 */
interface PayrollDeductionsSectionProps {
  readonly items: IncomeDeductionDraft[];
  readonly setItems: ((items: IncomeDeductionDraft[]) => void) | null;
}

/**
 * Edits the employee deductions in a payroll breakdown.
 */
const PayrollDeductionsSection = function ({
  items,
  setItems,
}: PayrollDeductionsSectionProps): JSX.Element {
  const update = (
    index: number,
    recipe: (item: IncomeDeductionDraft) => IncomeDeductionDraft,
  ): void => {
    setItems?.(
      items.map((item, itemIndex) =>
        itemIndex === index ? recipe(item) : item,
      ),
    );
  };
  return (
    <Stack spacing={1.5}>
      <PayrollSectionHeading
        title="Employee Deductions"
        description="Employee-funded amounts removed from gross compensation."
        action={
          <PayrollAddButton
            label="Add Deduction"
            onClick={
              setItems === null
                ? null
                : () => {
                    setItems([
                      ...items,
                      {
                        description: null,
                        amount: null,
                        disposition: 0,
                        reducesTaxableWagesFor: 0,
                      },
                    ]);
                  }
            }
          />
        }
      />
      {items.length === 0 && setItems === null ? (
        <PayrollEmptyState />
      ) : (
        <Box
          sx={{
            display: "grid",
            gap: 1.5,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 540px), 1fr))",
          }}
        >
          {items.map((item, index) => (
            <PayrollItemShell
              key={`deduction-${index}`}
              onDelete={
                setItems === null
                  ? null
                  : () => {
                      setItems(
                        items.filter((_, itemIndex) => itemIndex !== index),
                      );
                    }
              }
            >
              <Box
                sx={{
                  display: "grid",
                  gap: 1.5,
                  gridTemplateColumns: {
                    xs: "1fr",
                    md: "minmax(0, 1.5fr) minmax(160px, 1fr) minmax(220px, 1fr)",
                  },
                }}
              >
                <StringEntryField
                  label="Description"
                  value={item.description}
                  setValue={
                    setItems === null
                      ? null
                      : (description) => {
                          update(index, (current) => ({
                            ...current,
                            description,
                          }));
                        }
                  }
                />
                <CurrencyEntryField
                  label="Amount"
                  value={item.amount}
                  setValue={
                    setItems === null
                      ? null
                      : (amount) => {
                          update(index, (current) => ({ ...current, amount }));
                        }
                  }
                />
                <TextField
                  select
                  label="Disposition"
                  value={item.disposition}
                  disabled={setItems === null}
                  onChange={(event) => {
                    update(index, (current) => ({
                      ...current,
                      disposition: Number(event.target.value),
                    }));
                  }}
                >
                  <MenuItem value={0}>Reduces income</MenuItem>
                  <MenuItem value={1}>Untracked contribution</MenuItem>
                </TextField>
              </Box>
              <PayrollTaxTreatmentSelector
                labelPrefix="Reduces "
                value={item.reducesTaxableWagesFor}
                setValue={
                  setItems === null
                    ? null
                    : (reducesTaxableWagesFor) => {
                        update(index, (current) => ({
                          ...current,
                          reducesTaxableWagesFor,
                        }));
                      }
                }
              />
            </PayrollItemShell>
          ))}
        </Box>
      )}
    </Stack>
  );
};

export default PayrollDeductionsSection;
