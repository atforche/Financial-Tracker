import type { Fund, FundIdentifier } from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionDestinationFormFrame component.
 */
interface FundTransactionDestinationFormFrameProps {
  readonly index: number;
  readonly funds: Fund[];
  readonly fund: Fund | null;
  readonly setFund: ((fund: Fund | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
  readonly onRemove?: (() => void) | null;
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
  onRemove = null,
}: FundTransactionDestinationFormFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onRemove={onRemove}
    >
      <FundEntryField
        label="Destination Fund"
        options={funds}
        value={fund}
        setValue={
          setFund === null
            ? null
            : (nextValue): void => {
                setFund(
                  funds.find((candidate) => candidate.id === nextValue?.id) ??
                    null,
                );
              }
        }
        filter={filter}
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
