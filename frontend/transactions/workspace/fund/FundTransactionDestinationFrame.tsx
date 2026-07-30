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
 * Props for the FundTransactionDestinationFrame component.
 */
interface FundTransactionDestinationFrameProps {
  readonly index: number;
  readonly funds: FundWithBalance[];
  readonly fund: FundBalanceEventDraft | null;
  readonly setFund: ((fund: FundBalanceEventDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly filter?: ((fund: Fund) => boolean) | null;
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
      <FundBalanceEventFrame
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
