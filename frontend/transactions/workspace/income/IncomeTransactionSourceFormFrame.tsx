import type { Account, AccountIdentifier } from "@/accounts/types";
import {
  type IncomeDeductionDraft,
  type IncomeLineDraft,
  createEmptyDeduction,
  createEmptyLine,
} from "@/transactions/workspace/income/helpers";
import AccountOrLocationEntryFrame from "@/transactions/workspace/AccountOrLocationEntryFrame";
import IncomeTransactionItemSection from "@/transactions/workspace/income/IncomeTransactionSourceItemFrame";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the IncomeTransactionSourceFrame component.
 */
interface IncomeTransactionSourceFrameProps {
  readonly accounts: Account[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly setIncomeLines: (incomeLines: IncomeLineDraft[]) => void;
  readonly incomeDeductions: IncomeDeductionDraft[];
  readonly setIncomeDeductions: (
    incomeDeductions: IncomeDeductionDraft[],
  ) => void;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays the source frame for an income transaction.
 */
const IncomeTransactionSourceFrame = function ({
  accounts,
  account,
  setAccount,
  location,
  setLocation,
  incomeLines,
  setIncomeLines,
  incomeDeductions,
  setIncomeDeductions,
  accountFilter = null,
}: IncomeTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Income Source">
      <AccountOrLocationEntryFrame
        accountCaption="Source Account"
        accounts={accounts}
        account={account}
        setAccount={setAccount}
        locationCaption="Source Location"
        location={location}
        setLocation={setLocation}
        accountFilter={accountFilter}
      />
      <IncomeTransactionItemSection
        title="Income Lines"
        description="Add the gross income amounts that make up this transaction."
        items={incomeLines}
        setItems={setIncomeLines}
        createEmptyItem={createEmptyLine}
        addLabel="Add Income Line"
        allowEmpty={false}
      />
      <IncomeTransactionItemSection
        title="Income Deductions"
        description="Add optional deductions withheld before the income is deposited."
        items={incomeDeductions}
        setItems={setIncomeDeductions}
        createEmptyItem={createEmptyDeduction}
        addLabel="Add Deduction"
        allowEmpty
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export type { IncomeLineDraft, IncomeDeductionDraft };
export default IncomeTransactionSourceFrame;
