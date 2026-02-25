import CurrencyEntryField from "@framework/dialog/CurrencyEntryField";
import type FundAmountEntryModel from "@funds/FundAmountEntryModel";
import FundEntryField from "@funds/FundEntryField";
import type { FundIdentifier } from "@funds/ApiTypes";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the FundAmountEntryFrame component.
 */
interface FundAmountEntryFrameProps {
  readonly value: FundAmountEntryModel;
  readonly setValue?: ((newValue: FundAmountEntryModel) => void) | null;
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
    <Stack
      direction="row"
      spacing={2}
      alignItems="center"
      sx={{ width: "100%" }}
    >
      <FundEntryField
        label="Fund"
        value={
          value.fundId !== null && value.fundName !== null
            ? { id: value.fundId, name: value.fundName }
            : null
        }
        setValue={
          setValue
            ? (newValue): void => {
                setValue({
                  fundId: newValue?.id ?? null,
                  fundName: newValue?.name ?? null,
                  amount: value.amount ?? null,
                });
              }
            : null
        }
        filter={filter}
      />
      <CurrencyEntryField
        label="Amount"
        value={value.amount}
        setValue={
          setValue
            ? (newAmount): void => {
                setValue({
                  fundId: value.fundId ?? null,
                  fundName: value.fundName ?? null,
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
