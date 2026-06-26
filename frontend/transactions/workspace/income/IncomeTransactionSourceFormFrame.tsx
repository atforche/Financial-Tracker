import type { Account, AccountIdentifier } from "@/accounts/types";
import {
  type IncomeDeductionDraft,
  type IncomeLineDraft,
  createEmptyDeduction,
  createEmptyLine,
} from "@/transactions/workspace/income/helpers";
import AccountEntryField from "@/accounts/AccountEntryField";
import IncomeTransactionItemSection from "@/transactions/workspace/income/IncomeTransactionSourceItemFrame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

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
    <TransactionFrame
      title="Income Source"
      description="Choose where the income originated and capture the gross lines and deductions that produce the net transaction amount."
    >
      <AccountEntryField
        label="Source Account"
        options={accounts}
        value={account}
        setValue={
          setAccount === null
            ? null
            : (nextValue): void => {
                setAccount(
                  accounts.find(
                    (candidate) => candidate.id === nextValue?.id,
                  ) ?? null,
                );
              }
        }
        filter={accountFilter}
      />
      <StringEntryField
        label="Source Location"
        value={location}
        setValue={account === null ? setLocation : null}
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
    </TransactionFrame>
  );
};

export type { IncomeLineDraft, IncomeDeductionDraft };
export default IncomeTransactionSourceFrame;
