"use client";

import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import { Box } from "@mui/material";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";

/**
 * Props for the AccountOpeningFrame component.
 */
interface AccountOpeningFrameProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod:
    ((accountingPeriod: AccountingPeriod | null) => void) | null;
  readonly accountingPeriodErrorMessage?: string | null;
  readonly dateOpened: Dayjs | null;
  readonly setDateOpened: ((dateOpened: Dayjs | null) => void) | null;
  readonly dateOpenedErrorMessage?: string | null;
  readonly color?: FrameColor;
}

/**
 * Displays the shared opening accounting period and date section for accounts.
 */
const AccountOpeningFrame = function ({
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod,
  accountingPeriodErrorMessage = null,
  dateOpened,
  setDateOpened,
  dateOpenedErrorMessage = null,
  color = "info",
}: AccountOpeningFrameProps): JSX.Element {
  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame title="Opening Details" color={color}>
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
          }}
        >
          <AccountingPeriodEntryField
            label="Opening Accounting Period"
            options={accountingPeriods}
            value={accountingPeriod}
            setValue={setAccountingPeriod}
            errorMessage={accountingPeriodErrorMessage}
          />
          <DateEntryField
            label="Date Opened"
            value={dateOpened}
            setValue={setDateOpened}
            errorMessage={dateOpenedErrorMessage}
            minDate={
              accountingPeriod === null
                ? null
                : getMinimumDate(accountingPeriod)
            }
            maxDate={
              accountingPeriod === null
                ? null
                : getMaximumDate(accountingPeriod)
            }
            disabled={accountingPeriod === null}
          />
        </Box>
      </Frame>
    </Box>
  );
};

export default AccountOpeningFrame;
