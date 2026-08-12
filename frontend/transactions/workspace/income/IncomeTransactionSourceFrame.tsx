import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import {
  type EmployerContributionDraft,
  type IncomeDeductionDraft,
  type IncomeLineDraft,
  type PayrollTaxWithholdingDraft,
  getNetIncomeAmount,
} from "@/transactions/workspace/income/helpers";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import PayrollIncomeDetails from "@/transactions/workspace/income/PayrollIncomeDetails";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the IncomeTransactionSourceFrame component.
 */
interface IncomeTransactionSourceFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly setIncomeLines: ((incomeLines: IncomeLineDraft[]) => void) | null;
  readonly incomeDeductions: IncomeDeductionDraft[];
  readonly setIncomeDeductions:
    ((incomeDeductions: IncomeDeductionDraft[]) => void) | null;
  readonly employerContributions: EmployerContributionDraft[];
  readonly setEmployerContributions:
    ((items: EmployerContributionDraft[]) => void) | null;
  readonly taxWithholdings: PayrollTaxWithholdingDraft[];
  readonly setTaxWithholdings:
    ((items: PayrollTaxWithholdingDraft[]) => void) | null;
  readonly stateIncomeStateCode: string | null;
  readonly setStateIncomeStateCode: ((value: string) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays the source frame for an income transaction.
 */
const IncomeTransactionSourceFrame = function ({
  accounts,
  transaction = null,
  account,
  setAccount,
  location,
  setLocation,
  incomeLines,
  setIncomeLines,
  incomeDeductions,
  setIncomeDeductions,
  employerContributions,
  setEmployerContributions,
  taxWithholdings,
  setTaxWithholdings,
  stateIncomeStateCode,
  setStateIncomeStateCode,
  accountFilter = null,
  color = "info",
  readOnly = false,
}: IncomeTransactionSourceFrameProps): JSX.Element {
  const balanceChange = -getNetIncomeAmount({
    account,
    location,
    incomeLines,
    incomeDeductions,
    employerContributions,
    taxWithholdings,
    stateIncomeStateCode,
  });

  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionAccountOrLocationFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountCaption="Source Account"
        locationCaption="Source Location"
        location={location}
        setLocation={readOnly ? null : setLocation}
        accountFilter={accountFilter}
        balanceChange={balanceChange}
        readOnly={readOnly}
      />
      <PayrollIncomeDetails
        stateIncomeStateCode={stateIncomeStateCode}
        setStateIncomeStateCode={readOnly ? null : setStateIncomeStateCode}
        earnings={incomeLines}
        setEarnings={readOnly ? null : setIncomeLines}
        deductions={incomeDeductions}
        setDeductions={readOnly ? null : setIncomeDeductions}
        contributions={employerContributions}
        setContributions={readOnly ? null : setEmployerContributions}
        withholdings={taxWithholdings}
        setWithholdings={readOnly ? null : setTaxWithholdings}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export type { IncomeLineDraft, IncomeDeductionDraft };
export default IncomeTransactionSourceFrame;
