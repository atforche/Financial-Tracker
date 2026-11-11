import type {
  Account,
  AccountBalanceEvent,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { Box, Stack } from "@mui/material";
import type { FundBalanceEvent, FundWithBalance } from "@/funds/types";
import type {
  FundGoalBalanceEvent,
  FundGoalWithProgress,
} from "@/fund-goals/types";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/transactions/workspace/TransactionSection";
import type { FundAssignmentDraft } from "@/funds/assignmentPlanner/helpers";
import IncomeFundAssignmentPlanner from "@/funds/assignmentPlanner/IncomeFundAssignmentPlanner";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";
import { isTrackedAccount } from "@/accounts/helpers";

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
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly fundAssignmentsValid?: boolean;
  readonly accountBalanceEvent?: AccountBalanceEvent | null;
  readonly fundBalanceEvents?: FundBalanceEvent[] | undefined;
  readonly fundGoalBalanceEvents?: FundGoalBalanceEvent[] | undefined;
  readonly isPlanned: boolean;
  readonly readOnly?: boolean;
  readonly autoFocus?: boolean;
}

const emptyFundBalanceEvents: FundBalanceEvent[] = [];
const emptyFundGoalBalanceEvents: FundGoalBalanceEvent[] = [];

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
  onRemove = null,
  color = "info",
  fundAssignmentsValid = true,
  accountBalanceEvent = null,
  fundBalanceEvents = emptyFundBalanceEvents,
  fundGoalBalanceEvents = emptyFundGoalBalanceEvents,
  isPlanned,
  readOnly = false,
  autoFocus = false,
}: IncomeTransactionDestinationFrameProps): JSX.Element {
  const supportsFundAssignments = isTrackedAccount(account) === true;

  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onRemove={readOnly ? null : onRemove}
      color={fundAssignmentsValid ? color : "error"}
    >
      <Stack
        // Viewing keeps the name and amount on one row on phones, since neither needs an input's width
        direction={{ xs: readOnly ? "row" : "column", sm: "row" }}
        spacing={2}
        alignItems={{
          xs: readOnly ? "flex-start" : "stretch",
          sm: "flex-start",
        }}
      >
        <Box sx={{ flex: "1 1 auto", minWidth: 0 }}>
          <AccountBalanceEventFrame
            accounts={accounts}
            transaction={transaction}
            account={account}
            setAccount={readOnly ? null : setAccount}
            accountFilter={filter}
            label="Deposit Account"
            balanceEvent={accountBalanceEvent}
            autoFocus={autoFocus}
          />
        </Box>
        <CurrencyEntryField
          label="Destination Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{
            width: { xs: readOnly ? "auto" : "100%", sm: 220 },
            // Keeps a viewed amount whole beside a long name or balance chips
            flexShrink: 0,
            whiteSpace: "nowrap",
          }}
        />
      </Stack>
      {supportsFundAssignments ? (
        <IncomeFundAssignmentPlanner
          funds={funds}
          fundGoals={fundGoals}
          fundBalanceEvents={fundBalanceEvents}
          fundGoalBalanceEvents={fundGoalBalanceEvents}
          totalAmountToAssign={amount}
          fundAssignments={fundAssignments}
          setFundAssignments={readOnly ? null : setFundAssignments}
          baselineFundAssignments={baselineFundAssignments}
          isPlanned={isPlanned}
          readOnly={readOnly}
        />
      ) : null}
    </TransactionSourceOrDestinationFrame>
  );
};

export default IncomeTransactionDestinationFrame;
