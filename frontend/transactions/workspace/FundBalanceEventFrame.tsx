import type {
  Fund,
  FundBalanceEvent,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import { getSelectedTransactionFundDraft } from "@/transactions/workspace/fundBalanceEventDraft";

/**
 * Props for the FundBalanceEventFrame component.
 */
interface FundBalanceEventFrameProps {
  readonly funds?: FundWithBalance[];
  readonly fund: FundBalanceEventDraft | null;
  readonly setFund?: ((fund: FundBalanceEventDraft | null) => void) | null;
  readonly fundFilter?: ((fund: Fund) => boolean) | null;
  readonly label?: string;
  readonly balanceEvent?: FundBalanceEvent | null;
  readonly autoFocus?: boolean;
}

const emptyFunds: FundWithBalance[] = [];

/**
 * Displays a transaction fund in a workspace view. Shows the balance change from the provided balance
 * event once it has posted.
 */
const FundBalanceEventFrame = function ({
  funds = emptyFunds,
  fund,
  setFund = null,
  fundFilter = null,
  label = "Fund",
  balanceEvent = null,
  autoFocus = false,
}: FundBalanceEventFrameProps): JSX.Element {
  const selectedFund = funds.find(({ id }) => id === fund?.fundId) ?? null;
  const previousBalance = balanceEvent?.previousBalance ?? null;
  const newBalance = balanceEvent?.newBalance ?? null;

  return (
    <Stack spacing={0.75}>
      <FundEntryField
        label={label}
        options={funds}
        value={selectedFund}
        setValue={
          setFund === null
            ? null
            : (nextValue: Fund | null): void => {
                setFund(getSelectedTransactionFundDraft(funds, nextValue));
              }
        }
        filter={fundFilter}
        autoFocus={autoFocus}
      />
      {previousBalance === null || newBalance === null ? null : (
        <TransactionBalanceDetails
          previousPostedBalance={previousBalance.postedBalance}
          newPostedBalance={newBalance.postedBalance}
        />
      )}
    </Stack>
  );
};

export default FundBalanceEventFrame;
