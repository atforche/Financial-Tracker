/* eslint-disable @typescript-eslint/explicit-function-return-type -- JSX callbacks inherit the shared field callback signatures. */
import { Box, Stack } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { EmployerContributionDraft } from "@/transactions/workspace/income/helpers";
import type { JSX } from "react";
import PayrollAddButton from "@/transactions/workspace/income/PayrollAddButton";
import PayrollEmptyState from "@/transactions/workspace/income/PayrollEmptyState";
import PayrollItemShell from "@/transactions/workspace/income/PayrollItemShell";
import PayrollSectionHeading from "@/transactions/workspace/income/PayrollSectionHeading";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the PayrollEmployerContributionsSection component.
 */
interface PayrollEmployerContributionsSectionProps {
  readonly items: EmployerContributionDraft[];
  readonly setItems: ((items: EmployerContributionDraft[]) => void) | null;
}

/**
 * Edits the employer contributions in a payroll breakdown.
 */
const PayrollEmployerContributionsSection = function ({
  items,
  setItems,
}: PayrollEmployerContributionsSectionProps): JSX.Element {
  const update = (
    index: number,
    recipe: (item: EmployerContributionDraft) => EmployerContributionDraft,
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
        title="Employer Contributions"
        description="Employer-funded compensation that does not enter the cash deposit."
        action={
          <PayrollAddButton
            label="Add Employer Contribution"
            onClick={
              setItems === null
                ? null
                : () => {
                    setItems([...items, { description: null, amount: null }]);
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
              "repeat(auto-fit, minmax(min(100%, 360px), 1fr))",
          }}
        >
          {items.map((item, index) => (
            <PayrollItemShell
              key={`contribution-${index}`}
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
                    md: "minmax(0, 1.8fr) minmax(180px, 1fr)",
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
              </Box>
            </PayrollItemShell>
          ))}
        </Box>
      )}
    </Stack>
  );
};

export default PayrollEmployerContributionsSection;
