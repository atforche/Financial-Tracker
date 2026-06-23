import type { Fund, FundIdentifier } from "@/funds/types";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

interface FundTransactionSourceFrameProps {
  readonly funds: Fund[];
  readonly fund: Fund | null;
  readonly setFund: ((fund: Fund | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
}

/**
 * Displays the source frame for a fund transaction.
 */
const FundTransactionSourceFrame = function ({
  funds,
  fund,
  setFund,
  filter = null,
}: FundTransactionSourceFrameProps): JSX.Element {
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
    </TransactionFrame>
  );
};

export default FundTransactionSourceFrame;
