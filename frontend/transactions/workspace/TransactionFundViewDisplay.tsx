import type { Fund, FundIdentifier } from "@/funds/types";
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
 * Props for the TransactionFundViewDisplay component.
 */
interface TransactionFundViewDisplayProps {
  readonly funds?: Fund[];
  readonly fund: TransactionFundDraft | null;
  readonly setFund?: ((fund: TransactionFundDraft | null) => void) | null;
  readonly fundFilter?: ((fund: FundIdentifier) => boolean) | null;
  readonly label?: string;
  readonly balanceChange?: number | null;
}

const emptyFunds: Fund[] = [];

/**
 * Displays a transaction fund in a workspace view.
 */
const TransactionFundViewDisplay = function ({
  funds = emptyFunds,
  fund,
  setFund = null,
  fundFilter = null,
  label = "Fund",
  balanceChange = null,
}: TransactionFundViewDisplayProps): JSX.Element {
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

export default TransactionFundViewDisplay;
