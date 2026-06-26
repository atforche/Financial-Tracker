import type {
  TransactionAccount,
  TransactionFund,
} from "@/transactions/transaction";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import TransactionAccountViewDisplay from "@/transactions/workspace/TransactionAccountViewDisplay";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import TransactionFundAssignmentsViewSection from "@/transactions/workspace/TransactionFundAssignmentsViewSection";

/**
 * Props for the IncomeTransactionDestinationViewFrame component.
 */
interface IncomeTransactionDestinationViewFrameProps {
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
  index,
  funds,
  account,
  amount,
  fundAssignments,
}: IncomeTransactionDestinationViewFrameProps): JSX.Element {
  return (
    <TransactionFrame title={`Income Destination ${index + 1}`}>
      {account !== null && <TransactionAccountViewDisplay account={account} />}
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
