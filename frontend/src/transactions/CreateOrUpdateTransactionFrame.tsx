import dayjs, { type Dayjs } from "dayjs";
import AccountEntryField from "@accounts/AccountEntryField";
import type { AccountIdentifier } from "@accounts/ApiTypes";
import type { AccountingPeriodIdentifier } from "@accounting-periods/ApiTypes";
import DateEntryField from "@framework/dialog/DateEntryField";
import type { FundAmount } from "@funds/ApiTypes";
import FundAmountCollectionEntryFrame from "@funds/FundAmountCollectionEntryFrame";
import type { JSX } from "react";
import OpenAccountingPeriodEntryField from "@accounting-periods/OpenAccountingPeriodEntryField";
import { Stack } from "@mui/material";
import StringEntryField from "@framework/dialog/StringEntryField";

/**
 * Props for the CreateOrUpdateTransactionFrame component.
 */
interface CreateOrUpdateTransactionFrameProps {
  readonly accountingPeriod: AccountingPeriodIdentifier | null;
  readonly setAccountingPeriod:
    | ((newAccountingPeriod: AccountingPeriodIdentifier | null) => void)
    | null;
  readonly date: Dayjs | null;
  readonly setDate: ((newDate: Dayjs | null) => void) | null;
  readonly location: string;
  readonly setLocation: ((newLocation: string) => void) | null;
  readonly description: string;
  readonly setDescription: ((newDescription: string) => void) | null;
  readonly debitAccount: AccountIdentifier | null;
  readonly setDebitAccount:
    | ((newDebitAccount: AccountIdentifier | null) => void)
    | null;
  readonly debitFundAmounts: FundAmount[];
  readonly setDebitFundAmounts:
    | ((newDebitFundAmounts: FundAmount[]) => void)
    | null;
  readonly creditAccount: AccountIdentifier | null;
  readonly setCreditAccount:
    | ((newCreditAccount: AccountIdentifier | null) => void)
    | null;
  readonly creditFundAmounts: FundAmount[];
  readonly setCreditFundAmounts:
    | ((newCreditFundAmounts: FundAmount[]) => void)
    | null;
}

/**
 * Component that presents the user with fields to create or update a Transaction.
 * @param props - Props for the CreateOrUpdateTransactionFrame component.
 * @returns JSX element representing the CreateOrUpdateTransactionFrame component.
 */
const CreateOrUpdateTransactionFrame = function ({
  accountingPeriod,
  setAccountingPeriod,
  date,
  setDate,
  location,
  setLocation,
  description,
  setDescription,
  debitAccount,
  setDebitAccount,
  debitFundAmounts,
  setDebitFundAmounts,
  creditAccount,
  setCreditAccount,
  creditFundAmounts,
  setCreditFundAmounts,
}: CreateOrUpdateTransactionFrameProps): JSX.Element {
  return (
    <>
      <OpenAccountingPeriodEntryField
        label="Accounting Period"
        value={accountingPeriod}
        setValue={setAccountingPeriod}
      />
      <DateEntryField
        label="Date"
        value={date}
        setValue={setDate}
        minDate={
          accountingPeriod
            ? dayjs(accountingPeriod.name, "MMMM YYYY")
                .subtract(1, "month")
                .startOf("month")
            : null
        }
        maxDate={
          accountingPeriod
            ? dayjs(accountingPeriod.name, "MMMM YYYY")
                .add(2, "month")
                .subtract(1, "day")
            : null
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
      <Stack direction="row" spacing={2}>
        {setDebitFundAmounts ? (
          <Stack spacing={2}>
            <AccountEntryField
              label="Debit Account"
              value={debitAccount}
              setValue={setDebitAccount}
            />
            <FundAmountCollectionEntryFrame
              label="Debit Fund Amounts"
              value={debitFundAmounts}
              setValue={setDebitFundAmounts}
            />
          </Stack>
        ) : null}
        {setCreditFundAmounts ? (
          <Stack spacing={2}>
            <AccountEntryField
              label="Credit Account"
              value={creditAccount}
              setValue={setCreditAccount}
            />
            <FundAmountCollectionEntryFrame
              label="Credit Fund Amounts"
              value={creditFundAmounts}
              setValue={setCreditFundAmounts}
            />
          </Stack>
        ) : null}
      </Stack>
    </>
  );
};

export default CreateOrUpdateTransactionFrame;
