"use client";

import { Box, Typography } from "@mui/material";
import type { Dispatch, JSX, SetStateAction } from "react";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";

/**
 * Props for the AccountStartingBalanceFrame component.
 */
interface AccountStartingBalanceFrameProps {
  readonly value: number | null;
  readonly setValue: Dispatch<SetStateAction<number | null>> | null;
  readonly errorMessage?: string | null;
  readonly color?: FrameColor;
}

/**
 * Displays the shared starting balance section for account onboarding flows.
 */
const AccountStartingBalanceFrame = function ({
  value,
  setValue,
  errorMessage = null,
  color = "info",
}: AccountStartingBalanceFrameProps): JSX.Element {
  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame title="Starting Balance" color={color}>
        <Typography variant="body2" color="text.secondary">
          Set the balance that should be carried into the workspace today.
        </Typography>
        <CurrencyEntryField
          label="Starting Balance"
          value={value}
          setValue={setValue}
          errorMessage={errorMessage}
        />
      </Frame>
    </Box>
  );
};

export default AccountStartingBalanceFrame;
