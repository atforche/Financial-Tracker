import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import type { CreateTransactionFormSearchParams } from "@/app/transactions/create/createTransactionFormSearchParams";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the CreateTransactionDetailsFrame component.
 */
interface CreateTransactionDetailsFrameProps {
  readonly searchParams: CreateTransactionFormSearchParams;
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod: (
    accountingPeriod: AccountingPeriod | null,
  ) => void;
  readonly date: Dayjs | null;
  readonly setDate: (date: Dayjs | null) => void;
  readonly location: string;
  readonly setLocation: (location: string) => void;
  readonly description: string;
  readonly setDescription: (description: string) => void;
}

/**
 * Component that displays the entry fields for transaction details.
 */
const CreateTransactionDetailsFrame = function ({
  searchParams,
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod,
  date,
  setDate,
  location,
  setLocation,
  description,
  setDescription,
}: CreateTransactionDetailsFrameProps): JSX.Element {
  return (
    <Stack spacing={2} sx={{ maxWidth: "800px" }}>
      <AccountingPeriodEntryField
        label="Accounting Period"
        options={accountingPeriods}
        value={accountingPeriod}
        setValue={
          typeof searchParams.accountingPeriodId !== "undefined"
            ? null
            : setAccountingPeriod
        }
      />
      <DateEntryField
        label="Date"
        value={date ?? getDefaultDate(accountingPeriod)}
        setValue={setDate}
        minDate={
          accountingPeriod !== null ? getMinimumDate(accountingPeriod) : null
        }
        maxDate={
          accountingPeriod !== null ? getMaximumDate(accountingPeriod) : null
        }
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

export default CreateTransactionDetailsFrame;
