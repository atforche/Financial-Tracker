import type { AccountIdentifier, AccountWithBalance } from "@/accounts/types";
import {
  type IncomeDeductionDraft,
  type IncomeLineDraft,
  createEmptyDeduction,
  createEmptyLine,
  getNetIncomeAmount,
} from "@/transactions/workspace/income/helpers";
import type {
  Transaction,
  TransactionAccountDraft,
} from "@/transactions/transaction";
import type { FrameColor } from "@/framework/view/Frame";
import IncomeTransactionItemSection from "@/transactions/workspace/income/IncomeTransactionSourceItemFrame";
import type { JSX } from "react";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the IncomeTransactionSourceFrame component.
 */
interface IncomeTransactionSourceFrameProps {
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount:
    ((account: TransactionAccountDraft | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly setIncomeLines: ((incomeLines: IncomeLineDraft[]) => void) | null;
  readonly incomeDeductions: IncomeDeductionDraft[];
  readonly setIncomeDeductions:
    ((incomeDeductions: IncomeDeductionDraft[]) => void) | null;
  readonly accountFilter?: ((account: AccountIdentifier) => boolean) | null;
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
  accountFilter = null,
  color = "info",
  readOnly = false,
}: IncomeTransactionSourceFrameProps): JSX.Element {
  const balanceChange = -getNetIncomeAmount({
    account,
    location,
    incomeLines,
    incomeDeductions,
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
      <IncomeTransactionItemSection
        title="Income Lines"
        description="Add the gross income amounts that make up this transaction."
        items={incomeLines}
        setItems={readOnly ? null : setIncomeLines}
        createEmptyItem={readOnly ? null : createEmptyLine}
        addLabel={readOnly ? null : "Add Income Line"}
        allowEmpty={false}
      />
      <IncomeTransactionItemSection
        title="Income Deductions"
        description="Add optional deductions withheld before the income is deposited."
        items={incomeDeductions}
        setItems={readOnly ? null : setIncomeDeductions}
        createEmptyItem={readOnly ? null : createEmptyDeduction}
        addLabel={readOnly ? null : "Add Deduction"}
        allowEmpty
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export type { IncomeLineDraft, IncomeDeductionDraft };
export default IncomeTransactionSourceFrame;
