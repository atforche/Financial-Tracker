import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundEntryField from "@/framework/forms/FundEntryField";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the FundAmountEntryFrame component.
 */
interface FundAmountEntryFrameProps {
  readonly funds: FundIdentifier[];
  readonly value: FundAmount;
  readonly setValue: (newValue: FundAmount) => void;
  readonly filter?: ((fund: FundIdentifier) => boolean) | null;
  readonly autoFocus?: boolean;
}

/**
 * Component that presents the user with an entry field where they can enter a Fund Amount.
 */
const FundAmountEntryFrame = function ({
  funds,
  value,
  setValue,
  filter = null,
  autoFocus = false,
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
        options={funds}
        value={
          value.fundId !== "" && value.fundName !== ""
            ? { id: value.fundId, name: value.fundName }
            : null
        }
        setValue={(newValue): void => {
          setValue({
            fundId: newValue?.id ?? "",
            fundName: newValue?.name ?? "",
            amount: value.amount,
          });
        }}
        filter={filter}
        autoFocus={autoFocus}
      />
      <CurrencyEntryField
        label="Amount"
        value={value.amount}
        setValue={(newAmount): void => {
          setValue({
            fundId: value.fundId,
            fundName: value.fundName,
            amount: newAmount,
          });
        }}
      />
    </Stack>
  );
};

export default FundAmountEntryFrame;
