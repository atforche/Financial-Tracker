import {
  type AccountingPeriod,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import { Box } from "@mui/material";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import TransactionSection from "@/transactions/workspace/TransactionSection";

/**
 * Props for the TransactionDetailsSection component.
 */
interface TransactionDetailsSectionProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accountingPeriod: AccountingPeriod | null;
  readonly setAccountingPeriod:
    ((accountingPeriod: AccountingPeriod | null) => void) | null;
  readonly date: Dayjs | null;
  readonly setDate: (date: Dayjs | null) => void;
  readonly descriptionValue: string;
  readonly setDescriptionValue: (description: string) => void;
}

/**
 * Displays the shared date, accounting period, and description fields for transaction forms.
 */
const TransactionDetailsSection = function ({
  accountingPeriods,
  accountingPeriod,
  setAccountingPeriod,
  date,
  setDate,
  descriptionValue,
  setDescriptionValue,
}: TransactionDetailsSectionProps): JSX.Element {
  return (
    <TransactionSection
      title="Transaction Details"
      description="Specify the high level information about the transaction."
    >
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
    </TransactionSection>
  );
};

export default TransactionDetailsSection;
