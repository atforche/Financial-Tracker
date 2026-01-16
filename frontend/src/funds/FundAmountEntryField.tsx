import { Box, Stack, Typography } from "@mui/material";
import type { Fund, FundAmount } from "@funds/ApiTypes";
import { type JSX, useState } from "react";
import CurrencyEntryField from "@framework/dialog/CurrencyEntryField";
import FundEntryField from "./FundEntryField";

/**
 * Props for the FundAmountEntryField component.
 * @param label - Label for this Fund Amount Entry Field.
 * @param value - Current value for this Fund Amount Entry Field.
 * @param setValue - Callback to update the value in this Fund Amount Entry Field. If null, this field is read-only.
 */
interface FundAmountEntryFieldProps {
  readonly label: string;
  readonly value: FundAmount | null;
  readonly setValue?: ((newValue: FundAmount | null) => void) | null;
}

/**
 * Component that presents the user with an entry field where they can enter a Fund Amount.
 * @param props - Props for the FundAmountEntryField component.
 * @returns JSX element representing the FundAmountEntryField component.
 */
const FundAmountEntryField = function ({
  label,
  value,
  setValue = null,
}: FundAmountEntryFieldProps): JSX.Element {
  const [fund, setFund] = useState<Fund | null>(value ? value.fund : null);
  const [amount, setAmount] = useState<number>(value ? value.amount : 0);
  return (
    <>
      <Typography variant="body1" gutterBottom>
        {label}
      </Typography>
      <Box
        sx={{
          verticalAlign: "middle",
          border: "2px solid rgba(0, 0, 0, 0.12)",
          borderRadius: "5px",
          padding: "15px",
        }}
      >
        <Stack
          direction="row"
          spacing={2}
          alignItems="center"
          justifyContent="space-between"
        >
          <FundEntryField
            label="Fund"
            value={fund}
            setValue={
              setValue
                ? (newValue): void => {
                    setFund(newValue);
                    if (fund !== null && amount !== 0) {
                      setValue({ fund, amount });
                    }
                  }
                : null
            }
          />
          <CurrencyEntryField
            label="Amount"
            value={amount}
            setValue={
              setValue
                ? (newAmount): void => {
                    setAmount(newAmount);
                    if (fund !== null && amount !== 0) {
                      setValue({ fund, amount });
                    }
                  }
                : null
            }
          />
        </Stack>
      </Box>
    </>
  );
};

export default FundAmountEntryField;
