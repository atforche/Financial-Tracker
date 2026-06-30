import type {
  TransactionAccount,
  TransactionFund,
} from "@/transactions/transaction";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { Fund } from "@/funds/types";
import type { IncomeTransaction } from "@/transactions/incomeTransaction";
import type { JSX } from "react";
import TransactionAccountViewFrame from "@/transactions/workspace/TransactionAccountViewFrame";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import TransactionFundAssignmentsViewSection from "@/transactions/workspace/TransactionFundAssignmentsViewSection";

/**
 * Props for the IncomeTransactionDestinationViewFrame component.
 */
interface IncomeTransactionDestinationViewFrameProps {
  readonly transaction: IncomeTransaction;
  readonly index: number;
  readonly funds: Fund[];
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
  account,
  amount,
  fundAssignments,
}: IncomeTransactionDestinationViewFrameProps): JSX.Element {
  return (
    <TransactionFrame title={`Income Destination ${index + 1}`}>
      {account === null ? null : (
        <TransactionAccountViewFrame
          transaction={transaction}
          account={account}
        />
      )}
      <CurrencyEntryField label="Destination Amount" value={amount} />
      <TransactionFundAssignmentsViewSection
        funds={funds}
        amount={amount ?? 0}
        fundAssignments={fundAssignments}
        tone="income"
      />
    </TransactionFrame>
  );
};

export default IncomeTransactionDestinationViewFrame;
