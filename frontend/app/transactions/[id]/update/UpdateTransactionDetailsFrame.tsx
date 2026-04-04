import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the UpdateTransactionDetailsFrame component.
 */
interface UpdateTransactionDetailsFrameProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly date: Dayjs | null;
  readonly setDate: (date: Dayjs | null) => void;
  readonly location: string;
  readonly setLocation: (location: string) => void;
  readonly description: string;
  readonly setDescription: (description: string) => void;
  readonly dateErrors?: string | null;
}

/**
 * Component that displays the entry fields for transaction details.
 */
const UpdateTransactionDetailsFrame = function ({
  accountingPeriod,
  date,
  setDate,
  location,
  setLocation,
  description,
  setDescription,
  dateErrors = null,
}: UpdateTransactionDetailsFrameProps): JSX.Element {
  return (
    <Stack spacing={2} sx={{ maxWidth: "800px" }}>
      <AccountingPeriodEntryField
        label="Accounting Period"
        options={[accountingPeriod]}
        value={accountingPeriod}
      />
      <DateEntryField
        label="Date"
        value={date}
        setValue={setDate}
        minDate={getMinimumDate(accountingPeriod)}
        maxDate={getMaximumDate(accountingPeriod)}
        errorMessage={dateErrors ?? null}
      />
      <StringEntryField
        label="Location"
        value={location}
        setValue={setLocation}
      />
      <StringEntryField
        label="Description"
        value={description}
        setValue={setDescription}
      />
    </Stack>
  );
};

export default UpdateTransactionDetailsFrame;
