import type { Account, AccountIdentifier } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Fund, FundAmount } from "@/funds/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundAssignmentPlanner from "@/funds/FundAssignmentPlanner";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

interface SpendingTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly location: string;
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
const SpendingTransactionDestinationFrame = function ({
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
}: SpendingTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title={`SpendingTransactionDestination ${index + 1}`}
      description="Capture where this portion of the spending goes and how it should be funded."
      onRemove={onRemove}
    >
      <AccountEntryField
        label="Destination Account"
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
        filter={filter}
      />
      <StringEntryField
        label="Destination Location"
        value={location}
        setValue={account === null ? setLocation : null}
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
    </TransactionFrame>
  );
};

export default SpendingTransactionDestinationFrame;
