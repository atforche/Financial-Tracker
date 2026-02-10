import type { FundAmount, FundIdentifier } from "@funds/ApiTypes";
import CurrencyEntryField from "@framework/dialog/CurrencyEntryField";
import FundEntryField from "@funds/FundEntryField";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the FundAmountEntryFrame component.
 * @param value - Current value for this Fund Amount Entry Frame.
 * @param setValue - Callback to update the value in this Fund Amount Entry Frame. If null, this field is read-only.
 * @param filter - Function to filter the available funds for selection.
 */
interface FundAmountEntryFrameProps {
  readonly value: FundAmount | null;
  readonly setValue?: ((newValue: FundAmount | null) => void) | null;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
}

/**
 * Component that presents the user with an entry field where they can enter a Fund Amount.
 * @param props - Props for the FundAmountEntryFrame component.
 * @returns JSX element representing the FundAmountEntryFrame component.
 */
const FundAmountEntryFrame = function ({
  value,
  setValue = null,
  filter = null,
}: FundAmountEntryFrameProps): JSX.Element {
  return (
    <Stack direction="row" spacing={2} alignItems="center">
      <FundEntryField
        label="Fund"
        value={value ? { id: value.fundId, name: value.fundName } : null}
        setValue={
          setValue
            ? (newValue): void => {
                setValue({
                  fundId: newValue?.id ?? "",
                  fundName: newValue?.name ?? "",
                  amount: value?.amount ?? 0,
                });
              }
            : null
        }
        filter={filter}
      />
      <CurrencyEntryField
        label="Amount"
        value={value ? value.amount : null}
        setValue={
          setValue
            ? (newAmount): void => {
                setValue({
                  fundId: value?.fundId ?? "",
                  fundName: value?.fundName ?? "",
                  amount: newAmount,
                });
              }
            : null
        }
      />
    </Stack>
  );
};

export default FundAmountEntryFrame;
