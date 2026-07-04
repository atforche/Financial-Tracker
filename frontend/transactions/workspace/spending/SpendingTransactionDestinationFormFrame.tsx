import type { Account, AccountIdentifier } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Fund, FundAmount } from "@/funds/types";
import AccountOrLocationEntryFrame from "@/transactions/workspace/AccountOrLocationEntryFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the SpendingTransactionDestinationFormFrame component.
 */
interface SpendingTransactionDestinationFormFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string | null;
  readonly setLocation: ((location: string) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly fundAssignments: FundAmount[];
  readonly setFundAssignments: (fundAssignments: FundAmount[]) => void;
  readonly baselineFundAssignments?: FundAmount[];
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
  readonly onRemove?: (() => void) | null;
}

const emptyFundAssignments: FundAmount[] = [];

/**
 * Displays a destination frame for one spending destination.
 */
const SpendingTransactionDestinationFormFrame = function ({
  index,
  accounts,
  funds,
  assignmentGoals,
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
  onRemove = null,
}: SpendingTransactionDestinationFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onRemove={onRemove}
    >
      <AccountOrLocationEntryFrame
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
      <FundAssignmentPlanner
        title="Fund Assignments"
        tone="spending"
        funds={funds}
        assignmentGoals={assignmentGoals}
        spendingGoals={spendingGoals}
        totalAmountToAssign={amount}
        baselineValue={baselineFundAssignments}
        value={fundAssignments}
        setValue={setFundAssignments}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionDestinationFormFrame;
