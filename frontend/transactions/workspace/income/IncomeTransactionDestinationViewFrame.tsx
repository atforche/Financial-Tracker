import type {
  TransactionAccount,
  TransactionFund,
} from "@/transactions/transaction";
import type { AssignmentGoal } from "@/goals/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { Fund } from "@/funds/types";
import IncomeFundAssignmentPlanner from "@/funds/assignmentPlanner/IncomeFundAssignmentPlanner";
import type { IncomeTransaction } from "@/transactions/incomeTransaction";
import type { JSX } from "react";
import TransactionAccountViewFrame from "@/transactions/workspace/TransactionAccountViewFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";
import { getFundAssignmentFromTransactionFund } from "@/transactions/workspace/income/helpers";

/**
 * Props for the IncomeTransactionDestinationViewFrame component.
 */
interface IncomeTransactionDestinationViewFrameProps {
  readonly transaction: IncomeTransaction;
  readonly index: number;
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly account: TransactionAccount | null;
  readonly amount: number | null;
  readonly fundAssignments: TransactionFund[];
}

/**
 * Displays a view frame for one income transaction destination.
 */
const IncomeTransactionDestinationViewFrame = function ({
  transaction,
  index,
  funds,
  assignmentGoals,
  account,
  amount,
  fundAssignments,
}: IncomeTransactionDestinationViewFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Income Destination ${index + 1}`}
    >
      {account === null ? null : (
        <TransactionAccountViewFrame
          transaction={transaction}
          account={account}
        />
      )}
      <CurrencyEntryField label="Destination Amount" value={amount} />
      <IncomeFundAssignmentPlanner
        funds={funds}
        assignmentGoals={assignmentGoals}
        totalAmountToAssign={amount}
        fundAssignments={fundAssignments.map(
          getFundAssignmentFromTransactionFund,
        )}
        setFundAssignments={null}
        baselineFundAssignments={fundAssignments.map(
          getFundAssignmentFromTransactionFund,
        )}
        frameColor="success"
        readOnly
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default IncomeTransactionDestinationViewFrame;
