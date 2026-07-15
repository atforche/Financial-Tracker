import type { FundIdentifier, FundWithBalance } from "@/funds/types";
import {
  getSelectedTransactionFundDraft,
  setTransactionFundDraftBalanceChange,
} from "@/transactions/workspace/transactionFundDraft";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import type { TransactionFundDraft } from "@/transactions/transaction";

/**
 * Props for the TransactionFundFrame component.
 */
interface TransactionFundFrameProps {
  readonly funds?: FundWithBalance[];
  readonly fund: TransactionFundDraft | null;
  readonly setFund?: ((fund: TransactionFundDraft | null) => void) | null;
  readonly fundFilter?: ((fund: FundIdentifier) => boolean) | null;
  readonly label?: string;
  readonly balanceChange?: number | null;
}

const emptyFunds: FundWithBalance[] = [];

/**
 * Displays a transaction fund in a workspace view.
 */
const TransactionFundFrame = function ({
  funds = emptyFunds,
  fund,
  setFund = null,
  fundFilter = null,
  label = "Fund",
  balanceChange = null,
}: TransactionFundFrameProps): JSX.Element {
  const displayedFund = setTransactionFundDraftBalanceChange(
    fund,
    balanceChange,
  );

  return (
    <Stack spacing={0.75}>
      <FundEntryField
        label={label}
        options={funds}
        value={
          displayedFund === null
            ? null
            : {
                id: displayedFund.fundId ?? "",
                name: displayedFund.fundName ?? "",
              }
        }
        setValue={
          setFund === null
            ? null
            : (nextValue: FundIdentifier | null): void => {
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

export default TransactionFundFrame;
