"use client";

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
    <Frame title="Starting Balance" color={color}>
      <CurrencyEntryField
        label="Starting Balance"
        value={value}
        setValue={setValue}
        errorMessage={errorMessage}
      />
    </Frame>
  );
};

export default AccountStartingBalanceFrame;
