import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import type { ExpectedIncomeSource } from "@/accounting-periods/types";
import type { JSX } from "react";
import ReadOnlyField from "@/framework/forms/ReadOnlyField";

/**
 * Props for the ExpectedIncomeSourceEntryField component.
 */
interface ExpectedIncomeSourceEntryFieldProps {
  readonly expectedIncomeSources: readonly ExpectedIncomeSource[];
  readonly value: string | null;
  readonly setValue: ((expectedIncomeSourceId: string | null) => void) | null;
}

/**
 * Value of the option for income that was not planned as any Expected Income Source.
 */
const unplannedValue = "unplanned";

/**
 * Lets the user choose which Expected Income Source an income transaction was planned as, or mark it as unplanned.
 */
const ExpectedIncomeSourceEntryField = function ({
  expectedIncomeSources,
  value,
  setValue,
}: ExpectedIncomeSourceEntryFieldProps): JSX.Element {
  // A transaction keeps its link to a source that was deleted after it was entered
  const getLabel = (expectedIncomeSourceId: string | null): string =>
    expectedIncomeSourceId === null
      ? "Unplanned income"
      : (expectedIncomeSources.find(
          (source) => source.id === expectedIncomeSourceId,
        )?.name ?? "Removed source");
  if (setValue === null) {
    return (
      <ReadOnlyField label="Expected Income Source" value={getLabel(value)} />
    );
  }
  return (
    <ComboBoxEntryField<string>
      label="Expected Income Source"
      options={[
        ...expectedIncomeSources.map((source) => ({
          label: source.name,
          value: source.id,
        })),
        { label: getLabel(null), value: unplannedValue },
      ]}
      value={{ label: getLabel(value), value: value ?? unplannedValue }}
      setValue={(option) => {
        setValue(
          option === null || option.value === unplannedValue
            ? null
            : option.value,
        );
      }}
    />
  );
};

export default ExpectedIncomeSourceEntryField;
