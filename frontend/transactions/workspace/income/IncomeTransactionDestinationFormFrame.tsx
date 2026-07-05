import type { Account, AccountIdentifier } from "@/accounts/types";
import type {
  Transaction,
  TransactionAccountDraft,
} from "@/transactions/transaction";
import type { AssignmentGoal } from "@/goals/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { Fund } from "@/funds/types";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import IncomeFundAssignmentPlanner from "@/funds/assignmentPlanner/IncomeFundAssignmentPlanner";
import type { JSX } from "react";
import TransactionAccountFrame from "@/transactions/workspace/TransactionAccountFrame";
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
  readonly transaction?: Transaction | null;
  readonly account: TransactionAccountDraft | null;
  readonly setAccount:
    ((account: TransactionAccountDraft | null) => void) | null;
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
  transaction = null,
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
      <TransactionAccountFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={setAccount}
        accountFilter={filter}
        label="Deposit Account"
        balanceChange={amount}
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
