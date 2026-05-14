"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import { Button, DialogActions, Stack } from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import { ToggleState } from "@/accounting-periods/AccountingPeriodViewListFrames";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/accounts/breadcrumbs";
import createAccount from "@/accounts/createAccount";
import routes from "@/accounts/routes";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly routeAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the URL to redirect the user to after successfully creating an account.
 */
const getRedirectUrl = function (
  routeAccountingPeriod: AccountingPeriod | null,
): string {
  if (routeAccountingPeriod !== null) {
    return accountingPeriodRoutes.detail(
      { id: routeAccountingPeriod.id },
      { display: ToggleState.Accounts },
    );
  }
  return routes.index({});
};

/**
 * Normalizes the opened date for the selected accounting period.
 */
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
 * Component that displays the form for creating an account.
 */
const CreateAccountForm = function ({
  accountingPeriods,
  routeAccountingPeriod = null,
}: CreateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(routeAccountingPeriod);
  const [dateOpened, setDateOpened] = useState<Dayjs | null>(
    getDefaultDate(routeAccountingPeriod),
  );

  const [state, action, pending] = useActionState(createAccount, {
    redirectUrl: getRedirectUrl(routeAccountingPeriod),
  });

  const onAccountingPeriodChange = function (
    newAccountingPeriod: AccountingPeriod | null,
  ): void {
    setAccountingPeriod(newAccountingPeriod);
    setDateOpened((currentDateOpened) =>
      getNormalizedDateOpened(newAccountingPeriod, currentDateOpened),
    );
  };

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
    <Stack spacing={2}>
      <Breadcrumbs breadcrumbs={breadcrumbs.create(routeAccountingPeriod)} />
      <Stack spacing={2} sx={{ maxWidth: "500px" }}>
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
        <AccountingPeriodEntryField
          label="Opening Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            routeAccountingPeriod === null ? onAccountingPeriodChange : null
          }
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
        <DialogActions>
          <Link href={getRedirectUrl(routeAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={request === null}
            onClick={() => {
              if (request === null) {
                return;
              }
              startTransition(() => {
                action(request);
              });
            }}
          >
            Create
          </Button>
        </DialogActions>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Stack>
  );
};

export default CreateAccountForm;
