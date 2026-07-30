import type {
  Fund,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import {
  getSelectedTransactionFundDraft,
  setTransactionFundDraftBalanceChange,
} from "@/transactions/workspace/fundBalanceEventDraft";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";

/**
 * Props for the FundBalanceEventFrame component.
 */
interface FundBalanceEventFrameProps {
  readonly funds?: FundWithBalance[];
  readonly fund: FundBalanceEventDraft | null;
  readonly setFund?: ((fund: FundBalanceEventDraft | null) => void) | null;
  readonly fundFilter?: ((fund: Fund) => boolean) | null;
  readonly label?: string;
  readonly balanceChange?: number | null;
}

const emptyFunds: FundWithBalance[] = [];

/**
 * Displays a transaction fund in a workspace view.
 */
const FundBalanceEventFrame = function ({
  funds = emptyFunds,
  fund,
  setFund = null,
  fundFilter = null,
  label = "Fund",
  balanceChange = null,
}: FundBalanceEventFrameProps): JSX.Element {
  const displayedFund = setTransactionFundDraftBalanceChange(
    fund,
    balanceChange,
  );

  return (
    <Stack spacing={0.75}>
      <FundEntryField
        label={label}
        options={funds}
        value={funds.find(({ id }) => id === displayedFund?.fundId) ?? null}
        setValue={
          setFund === null
            ? null
            : (nextValue: Fund | null): void => {
                setFund(
                  getSelectedTransactionFundDraft(
                    funds,
                    nextValue,
                    fund,
                    balanceChange,
                  ),
                );
              }
        }
        filter={fundFilter}
      />
      <TransactionBalanceDetails
        previousPostedBalance={displayedFund?.previousFundBalance ?? 0}
        newPostedBalance={displayedFund?.newFundBalance ?? 0}
      />
    </Stack>
  );
};

export default FundBalanceEventFrame;
