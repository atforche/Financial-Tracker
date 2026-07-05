import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { JSX, ReactNode } from "react";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import { Box } from "@mui/material";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the TransactionDetailsFrame component.
 */
interface TransactionDetailsFrameProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod:
    ((accountingPeriod: AccountingPeriod | null) => void) | null;
  readonly date: Dayjs | null;
  readonly setDate: ((date: Dayjs | null) => void) | null;
  readonly descriptionValue: string;
  readonly setDescriptionValue: ((description: string) => void) | null;
  readonly headerContent?: ReactNode;
  readonly color?: FrameColor;
}

/**
 * Displays the shared date, accounting period, and description fields for transaction forms.
 */
const TransactionDetailsFrame = function ({
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod,
  date,
  setDate,
  descriptionValue,
  setDescriptionValue,
  headerContent,
  color = "info",
}: TransactionDetailsFrameProps): JSX.Element {
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
          <AccountingPeriodEntryField
            label="Accounting Period"
            options={accountingPeriods}
            value={accountingPeriod}
            setValue={setAccountingPeriod}
          />
          <DateEntryField
            label="Date"
            value={date}
            setValue={setDate}
            minDate={accountingPeriod ? getMinimumDate(accountingPeriod) : null}
            maxDate={accountingPeriod ? getMaximumDate(accountingPeriod) : null}
          />
        </Box>
        <StringEntryField
          label="Description"
          value={descriptionValue}
          setValue={setDescriptionValue}
        />
      </Frame>
    </Box>
  );
};

export default TransactionDetailsFrame;
