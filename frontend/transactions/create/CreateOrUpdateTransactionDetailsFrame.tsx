import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the CreateOrUpdateTransactionDetailsFrame component.
 */
interface CreateOrUpdateTransactionDetailsFrameProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod:
    | ((accountingPeriod: AccountingPeriod | null) => void)
    | null;
  readonly date: Dayjs | null;
  readonly setDate: (date: Dayjs | null) => void;
  readonly location: string;
  readonly setLocation: (location: string) => void;
  readonly description: string;
  readonly setDescription: (description: string) => void;
  readonly amount: number | null;
  readonly setAmount: (amount: number | null) => void;
}

/**
 * Components that displays the shared transaction details when creating or updating a Transaction.
 */
const CreateOrUpdateTransactionDetailsFrame = function ({
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod,
  date,
  setDate,
  location,
  setLocation,
  description,
  setDescription,
  amount,
  setAmount,
}: CreateOrUpdateTransactionDetailsFrameProps): JSX.Element {
  return (
    <CaptionedFrame caption="Details">
      <Stack spacing={2} sx={{ marginTop: 2 }}>
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
        <CurrencyEntryField
          label="Amount"
          value={amount}
          setValue={setAmount}
        />
      </Stack>
    </CaptionedFrame>
  );
};

export default CreateOrUpdateTransactionDetailsFrame;
