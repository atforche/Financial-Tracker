import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import IncomeFundAssignmentPlanner from "@/funds/assignmentPlanner/IncomeFundAssignmentPlanner";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";
import { isTrackedAccountType } from "@/accounts/helpers";

const emptyFundAmounts: FundAssignmentDraft[] = [];

/**
 * Props for the IncomeTransactionDestinationFrame component.
 */
interface IncomeTransactionDestinationFrameProps {
  readonly index: number;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly transaction?: Transaction | null;
  readonly account: AccountBalanceEventDraft | null;
  readonly setAccount:
    ((account: AccountBalanceEventDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments?: FundAssignmentDraft[];
  readonly filter?: ((account: Account) => boolean) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly fundAssignmentsValid?: boolean;
  readonly readOnly?: boolean;
}

/**
 * Displays a destination frame for one income destination.
 */
const IncomeTransactionDestinationFrame = function ({
  index,
  accounts,
  funds,
  fundGoals,
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
  readOnly = false,
}: IncomeTransactionDestinationFrameProps): JSX.Element {
  const supportsFundAssignments =
    account?.accountType !== null &&
    account?.accountType !== undefined &&
    isTrackedAccountType(account.accountType);

  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={readOnly ? null : onAdd}
      onRemove={readOnly ? null : onRemove}
      color={color}
    >
      <AccountBalanceEventFrame
        accounts={accounts}
        transaction={transaction}
        account={account}
        setAccount={readOnly ? null : setAccount}
        accountFilter={filter}
        label="Deposit Account"
        balanceChange={amount}
      />
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
      {supportsFundAssignments ? (
        <IncomeFundAssignmentPlanner
          funds={funds}
          fundGoals={fundGoals}
          totalAmountToAssign={amount}
          fundAssignments={fundAssignments}
          setFundAssignments={readOnly ? null : setFundAssignments}
          baselineFundAssignments={baselineFundAssignments}
          frameColor={fundAssignmentsValid ? "info" : "error"}
          readOnly={readOnly}
        />
      ) : null}
    </TransactionSourceOrDestinationFrame>
  );
};

export default IncomeTransactionDestinationFrame;
