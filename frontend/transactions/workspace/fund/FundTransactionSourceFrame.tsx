import type { Fund, FundIdentifier } from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import type { TransactionFundDraft } from "@/transactions/transaction";
import TransactionFundFrame from "@/transactions/workspace/TransactionFundFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionSourceFrame component.
 */
interface FundTransactionSourceFrameProps {
  readonly funds: Fund[];
  readonly fund: TransactionFundDraft | null;
  readonly setFund: ((fund: TransactionFundDraft | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays the form frame for a fund transaction source.
 */
const FundTransactionSourceFrame = function ({
  funds,
  fund,
  setFund,
  filter = null,
  amount,
  setAmount,
  color = "info",
  readOnly = false,
}: FundTransactionSourceFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame title="Source" color={color}>
      <TransactionFundFrame
        funds={funds}
        fund={fund}
        setFund={readOnly ? null : setFund}
        fundFilter={filter}
        label="Source Fund"
        balanceChange={amount === null ? null : -amount}
      />
      <CurrencyEntryField
        label="Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default FundTransactionSourceFrame;
