"use client";

import type { Dispatch, JSX, ReactNode, SetStateAction } from "react";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { AccountType } from "@/accounts/types";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import { Box } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the AccountDetailsFrame component.
 */
interface AccountDetailsFrameProps {
  readonly color?: FrameColor;
  readonly name: string;
  readonly setName?: Dispatch<SetStateAction<string>> | null;
  readonly nameErrorMessage?: string | null;
  readonly accountType: AccountType | null;
  readonly setAccountType?: ((newValue: AccountType | null) => void) | null;
  readonly accountTypeErrorMessage?: string | null;
  readonly headerContent?: ReactNode;
}

/**
 * Displays the shared account name and type section used across account flows.
 */
const AccountDetailsFrame = function ({
  color = "info",
  name,
  setName = null,
  nameErrorMessage = null,
  accountType,
  setAccountType = null,
  accountTypeErrorMessage = null,
  headerContent = null,
}: AccountDetailsFrameProps): JSX.Element {
  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame title="Details" color={color} headerContent={headerContent}>
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
          }}
        >
          <StringEntryField
            label="Name"
            value={name}
            setValue={setName}
            errorMessage={nameErrorMessage}
          />
          <AccountTypeEntryField
            label="Type"
            value={accountType}
            setValue={setAccountType}
            errorMessage={accountTypeErrorMessage}
          />
        </Box>
      </Frame>
    </Box>
  );
};

export default AccountDetailsFrame;
