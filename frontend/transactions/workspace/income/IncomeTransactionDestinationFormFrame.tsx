import type { Account, AccountIdentifier } from "@/accounts/types";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Fund, FundAmount } from "@/funds/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundAssignmentPlanner from "@/funds/FundAssignmentPlanner";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

const emptyFundAmounts: FundAmount[] = [];

/**
 * Props for the IncomeTransactionDestinationFormFrame component.
 */
interface IncomeTransactionDestinationFormFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly fundAssignments: FundAmount[];
  readonly setFundAssignments: (fundAssignments: FundAmount[]) => void;
  readonly baselineFundAssignments?: FundAmount[];
  readonly filter?: ((account: AccountIdentifier) => boolean) | null;
  readonly onRemove?: (() => void) | null;
}

/**
 * Displays a destination frame for one income destination.
 */
const IncomeTransactionDestinationFormFrame = function ({
  index,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  account,
  setAccount,
  amount,
  setAmount,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments = emptyFundAmounts,
  filter = null,
  onRemove = null,
}: IncomeTransactionDestinationFormFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title={`Income Destination ${index + 1}`}
      description="Capture which tracked account receives this portion of the income and how it should be allocated."
      onRemove={onRemove}
    >
      <AccountEntryField
        label="Deposit Account"
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
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={setAmount}
      />
      <FundAssignmentPlanner
        title="Fund Assignments"
        tone="income"
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

export default IncomeTransactionDestinationFormFrame;
