import type {
  EmployerContributionDraft,
  IncomeDeductionDraft,
  IncomeLineDraft,
  PayrollTaxWithholdingDraft,
} from "@/transactions/workspace/income/helpers";
import type { JSX } from "react";
import PayrollDeductionsSection from "@/transactions/workspace/income/PayrollDeductionsSection";
import PayrollEarningsSection from "@/transactions/workspace/income/PayrollEarningsSection";
import PayrollEmployerContributionsSection from "@/transactions/workspace/income/PayrollEmployerContributionsSection";
import PayrollTaxWithholdingsSection from "@/transactions/workspace/income/PayrollTaxWithholdingsSection";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the PayrollIncomeDetails component.
 */
interface PayrollIncomeDetailsProps {
  readonly stateIncomeStateCode: string | null;
  readonly setStateIncomeStateCode: ((value: string) => void) | null;
  readonly showStateField?: boolean;
  readonly earnings: IncomeLineDraft[];
  readonly setEarnings: ((items: IncomeLineDraft[]) => void) | null;
  readonly deductions: IncomeDeductionDraft[];
  readonly setDeductions: ((items: IncomeDeductionDraft[]) => void) | null;
  readonly contributions: EmployerContributionDraft[];
  readonly setContributions:
    ((items: EmployerContributionDraft[]) => void) | null;
  readonly withholdings: PayrollTaxWithholdingDraft[];
  readonly setWithholdings:
    ((items: PayrollTaxWithholdingDraft[]) => void) | null;
  readonly showWithholdings?: boolean;
}

/**
 * Composes the sections of an editable or read-only payroll breakdown.
 */
const PayrollIncomeDetails = function ({
  stateIncomeStateCode,
  setStateIncomeStateCode,
  showStateField = true,
  earnings,
  setEarnings,
  deductions,
  setDeductions,
  contributions,
  setContributions,
  withholdings,
  setWithholdings,
  showWithholdings = true,
}: PayrollIncomeDetailsProps): JSX.Element {
  return (
    <Stack spacing={3}>
      {showStateField ? (
        <StringEntryField
          label="State"
          value={stateIncomeStateCode}
          setValue={setStateIncomeStateCode}
        />
      ) : null}
      <PayrollEarningsSection items={earnings} setItems={setEarnings} />
      <PayrollDeductionsSection items={deductions} setItems={setDeductions} />
      <PayrollEmployerContributionsSection
        items={contributions}
        setItems={setContributions}
      />
      {showWithholdings ? (
        <PayrollTaxWithholdingsSection
          items={withholdings}
          setItems={setWithholdings}
        />
      ) : null}
    </Stack>
  );
};

export default PayrollIncomeDetails;
