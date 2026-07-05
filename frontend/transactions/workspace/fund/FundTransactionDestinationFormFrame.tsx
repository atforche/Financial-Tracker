import type { Fund, FundIdentifier } from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import type { TransactionFundDraft } from "@/transactions/transaction";
import TransactionFundViewDisplay from "@/transactions/workspace/TransactionFundViewDisplay";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionDestinationFormFrame component.
 */
interface FundTransactionDestinationFormFrameProps {
  readonly index: number;
  readonly funds: Fund[];
  readonly fund: TransactionFundDraft | null;
  readonly setFund: ((fund: TransactionFundDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
}

/**
 * Displays a form frame for one fund transaction destination.
 */
const FundTransactionDestinationFormFrame = function ({
  index,
  funds,
  fund,
  setFund,
  amount,
  setAmount,
  filter = null,
  onAdd = null,
  onRemove = null,
  color = "info",
}: FundTransactionDestinationFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={onAdd}
      onRemove={onRemove}
      color={color}
    >
      <TransactionFundViewDisplay
        funds={funds}
        fund={fund}
        setFund={setFund}
        fundFilter={filter}
        label="Destination Fund"
        balanceChange={amount}
      />
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={setAmount}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default FundTransactionDestinationFormFrame;
