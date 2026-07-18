"use client";

import Frame, { type FrameColor } from "@/framework/view/Frame";
import { getMaximumDate, getMinimumDate } from "@/accounting-periods/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";

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
    <ConstrainedContent maxWidth={1200}>
      <Frame title="Opening Details" color={color}>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
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
        </ResponsiveGrid>
      </Frame>
    </ConstrainedContent>
  );
};

export default AccountOpeningFrame;
