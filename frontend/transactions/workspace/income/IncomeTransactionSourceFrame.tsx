import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import {
  type IncomeDeductionDraft,
  type IncomeLineDraft,
  createEmptyDeduction,
  createEmptyLine,
  getNetIncomeAmount,
} from "@/transactions/workspace/income/helpers";
import type { Location, LocationDraft } from "@/locations/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import IncomeTransactionItemSection from "@/transactions/workspace/income/IncomeTransactionSourceItemFrame";
import type { JSX } from "react";
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
  readonly locations?: readonly Location[] | undefined;
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly incomeLines: IncomeLineDraft[];
  readonly setIncomeLines: ((incomeLines: IncomeLineDraft[]) => void) | null;
  readonly incomeDeductions: IncomeDeductionDraft[];
  readonly setIncomeDeductions:
    ((incomeDeductions: IncomeDeductionDraft[]) => void) | null;
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
  locations,
  setLocation,
  incomeLines,
  setIncomeLines,
  incomeDeductions,
  setIncomeDeductions,
  accountFilter = null,
  color = "info",
  readOnly = false,
}: IncomeTransactionSourceFrameProps): JSX.Element {
  const netIncomeAmount = getNetIncomeAmount({
    account,
    location,
    incomeLines,
    incomeDeductions,
  });
  const balanceChange = -netIncomeAmount;

  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionAccountOrLocationFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountCaption="Source Account"
        locationCaption="Source Location"
        locations={locations}
        location={location}
        setLocation={readOnly ? null : setLocation}
        accountFilter={accountFilter}
        balanceChange={balanceChange}
        readOnly={readOnly}
      />
      <IncomeTransactionItemSection
        title="Income Lines"
        items={incomeLines}
        setItems={readOnly ? null : setIncomeLines}
        createEmptyItem={readOnly ? null : createEmptyLine}
        addLabel={readOnly ? null : "Add Income Line"}
        allowEmpty={false}
      />
      <IncomeTransactionItemSection
        title="Income Deductions"
        items={incomeDeductions}
        setItems={readOnly ? null : setIncomeDeductions}
        createEmptyItem={readOnly ? null : createEmptyDeduction}
        addLabel={readOnly ? null : "Add Deduction"}
        allowEmpty
      />
      <CurrencyEntryField label="Net Income" value={netIncomeAmount} />
    </TransactionSourceOrDestinationFrame>
  );
};

export type { IncomeLineDraft, IncomeDeductionDraft };
export default IncomeTransactionSourceFrame;
