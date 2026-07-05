import type { Account, AccountIdentifier } from "@/accounts/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { Fund } from "@/funds/types";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import type { JSX } from "react";
import SpendingFundAssignmentPlanner from "@/funds/assignmentPlanner/SpendingFundAssignmentPlanner";
import type { SpendingGoal } from "@/goals/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionDestinationFormFrame component.
 */
interface SpendingTransactionDestinationFormFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly spendingGoals: SpendingGoal[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments: (fundAssignments: FundAssignmentDraft[]) => void;
  readonly baselineFundAssignments?: FundAssignmentDraft[];
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly fundAssignmentsValid?: boolean;
}

const emptyFundAssignments: FundAssignmentDraft[] = [];

/**
 * Displays a destination frame for one spending destination.
 */
const SpendingTransactionDestinationFormFrame = function ({
  index,
  accounts,
  funds,
  spendingGoals,
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
}: SpendingTransactionDestinationFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={onAdd}
      onRemove={onRemove}
      color={color}
    >
      <TransactionAccountOrLocationFrame
        accountCaption="Destination Account"
        accounts={accounts}
        account={account}
        setAccount={setAccount}
        locationCaption="Destination Location"
        location={location}
        setLocation={setLocation}
        accountFilter={filter}
      />
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={setAmount}
      />
      <SpendingFundAssignmentPlanner
        funds={funds}
        spendingGoals={spendingGoals}
        totalAmountToAssign={amount}
        fundAssignments={fundAssignments}
        setFundAssignments={setFundAssignments}
        baselineFundAssignments={baselineFundAssignments}
        frameColor={fundAssignmentsValid ? "info" : "error"}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionDestinationFormFrame;
