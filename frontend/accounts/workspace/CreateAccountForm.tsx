"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import { Button, DialogActions, Divider, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/accounts/workspace/createAccount";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly redirectUrl: string;
}

const getNormalizedDateOpened = function (
  accountingPeriod: AccountingPeriod | null,
  dateOpened: Dayjs | null,
): Dayjs | null {
  if (accountingPeriod === null) {
    return null;
  }
  const minimumDate = getMinimumDate(accountingPeriod);
  const maximumDate = getMaximumDate(accountingPeriod);
  if (
    dateOpened === null ||
    dateOpened.isBefore(minimumDate) ||
    dateOpened.isAfter(maximumDate)
  ) {
    return getDefaultDate(accountingPeriod);
  }
  return dateOpened;
};

/**
 * Displays the inline create-account form for the workspace.
 */
const CreateAccountForm = function ({
  accountingPeriods,
  redirectUrl,
}: CreateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const [dateOpened, setDateOpened] = useState<Dayjs | null>(
    getDefaultDate(null),
  );

  const [state, action, pending] = useActionState(createAccount, {});

  const onAccountingPeriodChange = function (
    newAccountingPeriod: AccountingPeriod | null,
  ): void {
    setAccountingPeriod(newAccountingPeriod);
    setDateOpened((currentDateOpened) =>
      getNormalizedDateOpened(newAccountingPeriod, currentDateOpened),
    );
  };

  const reset = function (): void {
    setName("");
    setAccountType(null);
    setAccountingPeriod(null);
    setDateOpened(getDefaultDate(null));
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
  }, [state]);

  let request: CreateAccountRequest | null = null;
  if (
    name !== "" &&
    accountType !== null &&
    accountingPeriod !== null &&
    dateOpened !== null
  ) {
    request = {
      name,
      type: accountType,
      openingAccountingPeriodId: accountingPeriod.id,
      dateOpened: dateOpened.format("YYYY-MM-DD"),
    };
  }

  return (
    <Stack spacing={3}>
      <Stack spacing={2.5}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <AccountTypeEntryField
          label="Type"
          value={accountType}
          setValue={setAccountType}
          errorMessage={state.typeErrors ?? null}
        />
      </Stack>
      <Divider flexItem />
      <Stack spacing={2.5}>
        <AccountingPeriodEntryField
          label="Opening Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={onAccountingPeriodChange}
          errorMessage={state.accountingPeriodErrors ?? null}
        />
        <DateEntryField
          label="Date Opened"
          value={dateOpened}
          setValue={setDateOpened}
          errorMessage={state.dateOpenedErrors ?? null}
          minDate={
            accountingPeriod === null ? null : getMinimumDate(accountingPeriod)
          }
          maxDate={
            accountingPeriod === null ? null : getMaximumDate(accountingPeriod)
          }
          disabled={accountingPeriod === null}
        />
      </Stack>
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
      <DialogActions sx={{ px: 0, pb: 0 }}>
        <Button variant="outlined" onClick={reset}>
          Reset
        </Button>
        <Button
          variant="contained"
          loading={pending}
          disabled={request === null}
          onClick={() => {
            if (request === null) {
              return;
            }
            startTransition(() => {
              action({
                redirectUrl,
                request,
              });
            });
          }}
        >
          Create account
        </Button>
      </DialogActions>
    </Stack>
  );
};

export default CreateAccountForm;
