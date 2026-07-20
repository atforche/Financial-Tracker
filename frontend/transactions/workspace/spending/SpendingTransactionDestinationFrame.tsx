import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import type { FundPlanWithProgress } from "@/fund-plans/types";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import SpendingFundAssignmentPlanner from "@/funds/assignmentPlanner/SpendingFundAssignmentPlanner";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionDestinationFrame component.
 */
interface SpendingTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundPlans: FundPlanWithProgress[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments?: FundAssignmentDraft[];
  readonly filter?: ((account: Account) => boolean) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly fundAssignmentsValid?: boolean;
  readonly readOnly?: boolean;
}

const emptyFundAssignments: FundAssignmentDraft[] = [];

/**
 * Displays a destination frame for one spending destination.
 */
const SpendingTransactionDestinationFrame = function ({
  index,
  accounts,
  funds,
  fundPlans,
  transaction = null,
  account,
  setAccount,
  location,
  setLocation,
  amount,
  setAmount,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments = emptyFundAssignments,
  filter = null,
  onAdd = null,
  onRemove = null,
  color = "info",
  fundAssignmentsValid = true,
  readOnly = false,
}: SpendingTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={readOnly ? null : onAdd}
      onRemove={readOnly ? null : onRemove}
      color={color}
    >
      <TransactionAccountOrLocationFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountCaption="Destination Account"
        locationCaption="Destination Location"
        location={location}
        setLocation={readOnly ? null : setLocation}
        accountFilter={filter}
        balanceChange={amount}
        readOnly={readOnly}
      />
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
      <SpendingFundAssignmentPlanner
        funds={funds}
        fundPlans={fundPlans}
        totalAmountToAssign={amount}
        fundAssignments={fundAssignments}
        setFundAssignments={readOnly ? null : setFundAssignments}
        baselineFundAssignments={baselineFundAssignments}
        frameColor={fundAssignmentsValid ? "info" : "error"}
        readOnly={readOnly}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionDestinationFrame;
