import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import type { Location, LocationDraft } from "@/locations/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import SpendingFundAssignmentPlanner from "@/funds/assignmentPlanner/SpendingFundAssignmentPlanner";
import type { Transaction } from "@/transactions/types";
import TransactionAccountOrLocationFrame from "@/transactions/workspace/TransactionAccountOrLocationFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the RefundTransactionSourceFrame component.
 */
interface RefundTransactionSourceFrameProps {
  readonly index: number;
  readonly accounts: AccountWithBalance[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly locations?: readonly Location[];
  readonly location: LocationDraft | null;
  readonly setLocation: ((location: LocationDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly accountFilter?: ((account: Account) => boolean) | null;
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((assignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments?: FundAssignmentDraft[];
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

const emptyFundAssignments: FundAssignmentDraft[] = [];

/**
 * Displays the source frame for a refund transaction.
 */
const RefundTransactionSourceFrame = function ({
  index,
  accounts,
  transaction = null,
  account,
  setAccount,
  locations,
  location,
  setLocation,
  amount,
  setAmount,
  accountFilter = null,
  funds,
  fundGoals,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments = emptyFundAssignments,
  onAdd = null,
  onRemove = null,
  color = "info",
  readOnly = false,
}: RefundTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Source ${index + 1}`}
      color={color}
      onAdd={readOnly ? null : onAdd}
      onRemove={readOnly ? null : onRemove}
    >
      <TransactionAccountOrLocationFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountFilter={accountFilter}
        accountCaption="Source Account"
        locations={locations}
        location={location}
        setLocation={readOnly ? null : setLocation}
        locationCaption="Source Location"
        balanceChange={amount === null ? null : -amount}
        readOnly={readOnly}
      />
      <CurrencyEntryField
        label="Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
      <SpendingFundAssignmentPlanner
        funds={funds}
        fundGoals={fundGoals}
        assignmentEffect="refund"
        totalAmountToAssign={amount}
        fundAssignments={fundAssignments}
        setFundAssignments={readOnly ? null : setFundAssignments}
        baselineFundAssignments={baselineFundAssignments}
        frameColor={color}
        readOnly={readOnly}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default RefundTransactionSourceFrame;
