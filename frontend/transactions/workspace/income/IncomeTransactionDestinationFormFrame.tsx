import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import type { AssignmentGoal } from "@/goals/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { Fund } from "@/funds/types";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import IncomeFundAssignmentPlanner from "@/funds/assignmentPlanner/IncomeFundAssignmentPlanner";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

const emptyFundAmounts: FundAssignmentDraft[] = [];

/**
 * Props for the IncomeTransactionDestinationFormFrame component.
 */
interface IncomeTransactionDestinationFormFrameProps {
  readonly index: number;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly account: Account | null;
  readonly setAccount: ((account: Account | null) => void) | null;
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

/**
 * Displays a destination frame for one income destination.
 */
const IncomeTransactionDestinationFormFrame = function ({
  index,
  accounts,
  funds,
  assignmentGoals,
  account,
  setAccount,
  amount,
  setAmount,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments = emptyFundAmounts,
  filter = null,
  onAdd = null,
  onRemove = null,
  color = "info",
  fundAssignmentsValid = true,
}: IncomeTransactionDestinationFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={onAdd}
      onRemove={onRemove}
      color={color}
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
      <IncomeFundAssignmentPlanner
        funds={funds}
        assignmentGoals={assignmentGoals}
        totalAmountToAssign={amount}
        fundAssignments={fundAssignments}
        setFundAssignments={setFundAssignments}
        baselineFundAssignments={baselineFundAssignments}
        frameColor={fundAssignmentsValid ? "info" : "error"}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default IncomeTransactionDestinationFormFrame;
