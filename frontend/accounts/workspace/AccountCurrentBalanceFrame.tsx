"use client";

import type { Account } from "@/accounts/types";
import { Box } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";

/**
 * Props for the AccountCurrentBalanceFrame component.
 */
interface AccountCurrentBalanceFrameProps {
  readonly account: Account;
}

/**
 * Displays the current balance snapshot for a selected account.
 */
const AccountCurrentBalanceFrame = function ({
  account,
}: AccountCurrentBalanceFrameProps): JSX.Element {
  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame title="Current Balance" color="info">
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
          }}
        >
          <CurrencyEntryField
            label="Posted Balance"
            value={account.currentBalance.postedBalance}
            setValue={null}
            errorMessage={null}
          />
          <CurrencyEntryField
            label="Balance Including Pending"
            value={
              account.currentBalance.postedBalance -
              account.currentBalance.pendingDebitAmount +
              account.currentBalance.pendingCreditAmount
            }
            setValue={null}
            errorMessage={null}
          />
        </Box>
      </Frame>
    </Box>
  );
};

export default AccountCurrentBalanceFrame;
