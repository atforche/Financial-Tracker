import type { Fund, FundIdentifier } from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

interface FundTransactionDestinationFrameProps {
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
 * Displays a destination frame for one fund transaction destination.
 */
const FundTransactionDestinationFrame = function ({
  index,
  funds,
  fund,
  setFund,
  amount,
  setAmount,
  filter = null,
  onRemove = null,
}: FundTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title={`Transfer Destination ${index + 1}`}
      description="Capture where this portion of the transfer is going."
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
    </TransactionFrame>
  );
};

export default FundTransactionDestinationFrame;
