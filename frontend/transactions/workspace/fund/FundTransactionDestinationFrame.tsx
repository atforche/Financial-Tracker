import type { FundIdentifier, FundWithBalance } from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
import type { JSX } from "react";
import type { TransactionFundDraft } from "@/transactions/transaction";
import TransactionFundFrame from "@/transactions/workspace/TransactionFundFrame";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionDestinationFrame component.
 */
interface FundTransactionDestinationFrameProps {
  readonly index: number;
  readonly funds: FundWithBalance[];
  readonly fund: TransactionFundDraft | null;
  readonly setFund: ((fund: TransactionFundDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
  readonly onAdd?: (() => void) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays a form frame for one fund transaction destination.
 */
const FundTransactionDestinationFrame = function ({
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
  readOnly = false,
}: FundTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onAdd={readOnly ? null : onAdd}
      onRemove={readOnly ? null : onRemove}
      color={color}
    >
      <TransactionFundFrame
        funds={funds}
        fund={fund}
        setFund={readOnly ? null : setFund}
        fundFilter={filter}
        label="Destination Fund"
        balanceChange={amount}
      />
      <CurrencyEntryField
        label="Destination Amount"
        value={amount}
        setValue={readOnly ? null : setAmount}
      />
    </TransactionSourceOrDestinationFrame>
  );
};

export default FundTransactionDestinationFrame;
