import type {
  Fund,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import FundBalanceEventFrame from "@/transactions/workspace/FundBalanceEventFrame";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionSourceFrame component.
 */
interface FundTransactionSourceFrameProps {
  readonly funds: FundWithBalance[];
  readonly fund: FundBalanceEventDraft | null;
  readonly setFund: ((fund: FundBalanceEventDraft | null) => void) | null;
  readonly filter?: ((fund: Fund) => boolean) | null;
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
      <FundBalanceEventFrame
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
