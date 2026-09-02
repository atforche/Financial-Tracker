import type {
  Account,
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { Box, Chip, Stack } from "@mui/material";
import {
  type FundAssignmentDraft,
  getAssignedFundAmount,
  getRemainingFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import AccountBalanceEventFrame from "@/transactions/workspace/AccountBalanceEventFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import IncomeFundAssignmentPlanner from "@/funds/assignmentPlanner/IncomeFundAssignmentPlanner";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/types";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";
import { formatCurrency } from "@/framework/currencyHelpers";
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
  const assignedAmount = getAssignedFundAmount(fundAssignments);
  const remainingAmount = getRemainingFundAmount(amount, fundAssignments);

  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={readOnly ? null : onAdd}
      onRemove={readOnly ? null : onRemove}
      color={fundAssignmentsValid ? color : "error"}
      headerContent={
        supportsFundAssignments ? (
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
            <Chip
              color={remainingAmount === 0 ? "success" : "error"}
              label={`Unassigned ${formatCurrency(remainingAmount ?? 0)}`}
            />
          </Stack>
        ) : null
      }
    >
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        alignItems={{ xs: "stretch", sm: "flex-start" }}
      >
        <Box sx={{ flex: { sm: "1 1 auto" }, minWidth: 0 }}>
          <AccountBalanceEventFrame
            accounts={accounts}
            transaction={transaction}
            account={account}
            setAccount={readOnly ? null : setAccount}
            accountFilter={filter}
            label="Deposit Account"
            balanceChange={amount}
          />
        </Box>
        <CurrencyEntryField
          label="Destination Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{ width: { xs: "100%", sm: 220 } }}
        />
      </Stack>
      {supportsFundAssignments ? (
        <IncomeFundAssignmentPlanner
          funds={funds}
          fundGoals={fundGoals}
          totalAmountToAssign={amount}
          fundAssignments={fundAssignments}
          setFundAssignments={readOnly ? null : setFundAssignments}
          baselineFundAssignments={baselineFundAssignments}
          readOnly={readOnly}
        />
      ) : null}
    </TransactionSourceOrDestinationFrame>
  );
};

export default IncomeTransactionDestinationFrame;
