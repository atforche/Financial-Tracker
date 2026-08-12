/* eslint-disable @typescript-eslint/explicit-function-return-type -- JSX callbacks inherit the shared field callback signatures. */
import { Box, MenuItem, Stack, TextField } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { JSX } from "react";
import PayrollAddButton from "@/transactions/workspace/income/PayrollAddButton";
import PayrollEmptyState from "@/transactions/workspace/income/PayrollEmptyState";
import PayrollItemShell from "@/transactions/workspace/income/PayrollItemShell";
import PayrollSectionHeading from "@/transactions/workspace/income/PayrollSectionHeading";
import type { PayrollTaxWithholdingDraft } from "@/transactions/workspace/income/helpers";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the PayrollTaxWithholdingsSection component.
 */
interface PayrollTaxWithholdingsSectionProps {
  readonly items: PayrollTaxWithholdingDraft[];
  readonly setItems: ((items: PayrollTaxWithholdingDraft[]) => void) | null;
}

/**
 * Edits the tax withholdings in a payroll breakdown.
 */
const PayrollTaxWithholdingsSection = function ({
  items,
  setItems,
}: PayrollTaxWithholdingsSectionProps): JSX.Element {
  const update = (
    index: number,
    recipe: (item: PayrollTaxWithholdingDraft) => PayrollTaxWithholdingDraft,
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
        title="Tax Withholdings"
        description="Taxes withheld from cash compensation by jurisdiction and category."
        action={
          <PayrollAddButton
            label="Add Tax Withholding"
            onClick={
              setItems === null
                ? null
                : () => {
                    setItems([
                      ...items,
                      {
                        jurisdiction: {
                          countryCode: "US",
                          subdivisionCode: null,
                          locality: null,
                        },
                        taxType: 0,
                        amount: null,
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
              "repeat(auto-fit, minmax(min(100%, 520px), 1fr))",
          }}
        >
          {items.map((item, index) => (
            <PayrollItemShell
              key={`withholding-${index}`}
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
                    md: "repeat(3, minmax(0, 1fr))",
                  },
                }}
              >
                <StringEntryField
                  label="Country Code"
                  value={item.jurisdiction.countryCode}
                  setValue={
                    setItems === null
                      ? null
                      : (countryCode) => {
                          update(index, (current) => ({
                            ...current,
                            jurisdiction: {
                              ...current.jurisdiction,
                              countryCode,
                            },
                          }));
                        }
                  }
                />
                <StringEntryField
                  label="State / Subdivision"
                  value={item.jurisdiction.subdivisionCode}
                  setValue={
                    setItems === null
                      ? null
                      : (subdivisionCode) => {
                          update(index, (current) => ({
                            ...current,
                            jurisdiction: {
                              ...current.jurisdiction,
                              subdivisionCode,
                            },
                          }));
                        }
                  }
                />
                <StringEntryField
                  label="Locality"
                  value={item.jurisdiction.locality}
                  setValue={
                    setItems === null
                      ? null
                      : (locality) => {
                          update(index, (current) => ({
                            ...current,
                            jurisdiction: { ...current.jurisdiction, locality },
                          }));
                        }
                  }
                />
                <TextField
                  select
                  label="Tax Type"
                  value={item.taxType}
                  disabled={setItems === null}
                  onChange={(event) => {
                    update(index, (current) => ({
                      ...current,
                      taxType: Number(event.target.value),
                    }));
                  }}
                >
                  <MenuItem value={0}>Income</MenuItem>
                  <MenuItem value={1}>Social Security</MenuItem>
                  <MenuItem value={2}>Medicare</MenuItem>
                  <MenuItem value={3}>Local</MenuItem>
                </TextField>
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
              </Box>
            </PayrollItemShell>
          ))}
        </Box>
      )}
    </Stack>
  );
};

export default PayrollTaxWithholdingsSection;
