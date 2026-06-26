import type { Fund, FundIdentifier } from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

/**
 * Props for the FundTransactionSourceFormFrame component.
 */
interface FundTransactionSourceFormFrameProps {
  readonly funds: Fund[];
  readonly fund: Fund | null;
  readonly setFund: ((fund: Fund | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
}

/**
 * Displays the form frame for a fund transaction source.
 */
const FundTransactionSourceFormFrame = function ({
  funds,
  fund,
  setFund,
  filter = null,
  amount,
  setAmount,
}: FundTransactionSourceFormFrameProps): JSX.Element {
  return (
    <TransactionFrame
      title="Transfer Source"
      description="Choose the source fund for this transfer."
    >
      <FundEntryField
        label="Source Fund"
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
      <CurrencyEntryField label="Amount" value={amount} setValue={setAmount} />
    </TransactionFrame>
  );
};

export default FundTransactionSourceFormFrame;
