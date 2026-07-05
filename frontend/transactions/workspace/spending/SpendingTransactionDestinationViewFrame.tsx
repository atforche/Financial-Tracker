import { Stack, Typography } from "@mui/material";
import type {
  TransactionAccountDraft,
  TransactionFund,
} from "@/transactions/transaction";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import SpendingFundAssignmentPlanner from "@/funds/assignmentPlanner/SpendingFundAssignmentPlanner";
import type { SpendingGoal } from "@/goals/types";
import type { SpendingTransaction } from "@/transactions/spendingTransaction";
import TransactionAccountFrame from "@/transactions/workspace/TransactionAccountFrame";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";
import { getFundAssignmentFromTransactionFund } from "@/transactions/workspace/spending/helpers";

/**
 * Props for the SpendingTransactionDestinationViewFrame component.
 */
interface SpendingTransactionDestinationViewFrameProps {
  readonly transaction: SpendingTransaction;
  readonly index: number;
  readonly funds: Fund[];
  readonly spendingGoals: SpendingGoal[];
  readonly account: TransactionAccountDraft | null;
  readonly location: string | null;
  readonly amount: number;
  readonly fundAssignments: TransactionFund[];
}

/**
 * Displays the read-only destination frame for one spending destination.
 */
const SpendingTransactionDestinationViewFrame = function ({
  transaction,
  index,
  funds,
  spendingGoals,
  account,
  location,
  amount,
  fundAssignments,
}: SpendingTransactionDestinationViewFrameProps): JSX.Element {
  const hasLocation = account === null && (location ?? "").trim() !== "";
  return (
    <TransactionSourceOrDestinationFrame title={`Destination ${index + 1}`}>
      <Stack spacing={2}>
        {account === null ? (
          <TransactionDisplayField label="Account" value="None" />
        ) : (
          <TransactionAccountFrame
            transaction={transaction}
            account={account}
          />
        )}

        {hasLocation ? (
          <>
            <Typography
              variant="body2"
              color="text.secondary"
              textAlign="center"
              textTransform="uppercase"
              letterSpacing="0.08em"
            >
              or
            </Typography>
            <TransactionDisplayField
              label="Destination Location"
              value={location ?? ""}
            />
          </>
        ) : null}

        <SpendingFundAssignmentPlanner
          funds={funds}
          spendingGoals={spendingGoals}
          totalAmountToAssign={amount}
          fundAssignments={fundAssignments.map(
            getFundAssignmentFromTransactionFund,
          )}
          setFundAssignments={null}
          baselineFundAssignments={fundAssignments.map(
            getFundAssignmentFromTransactionFund,
          )}
          frameColor="primary"
          readOnly
        />
      </Stack>
    </TransactionSourceOrDestinationFrame>
  );
};

export default SpendingTransactionDestinationViewFrame;
