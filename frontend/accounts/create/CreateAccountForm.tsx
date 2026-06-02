"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import {
  Button,
  DialogActions,
  Divider,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { type JSX, startTransition, useActionState, useState } from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/accounts/create/createAccount";
import routes from "@/accounts/routes";
import { useRouter } from "next/navigation";

/**
 * Gets the previous in-app URL, or falls back to the accounts index.
 */
const getReturnUrl = function (): string {
  const fallbackUrl = routes.index({});
  if (typeof window === "undefined") {
    return fallbackUrl;
  }
  const { referrer } = document;
  if (referrer === "") {
    return fallbackUrl;
  }
  try {
    const referrerUrl = new URL(referrer);
    if (referrerUrl.origin !== window.location.origin) {
      return fallbackUrl;
    }
    const currentUrl = new URL(window.location.href);
    const returnUrl = `${referrerUrl.pathname}${referrerUrl.search}${referrerUrl.hash}`;
    const currentPath = `${currentUrl.pathname}${currentUrl.search}${currentUrl.hash}`;
    return returnUrl !== "" && returnUrl !== currentPath
      ? returnUrl
      : fallbackUrl;
  } catch {
    return fallbackUrl;
  }
};

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
}

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
}: CreateAccountFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const [dateOpened, setDateOpened] = useState<Dayjs | null>(
    getDefaultDate(null),
  );

  const [state, action, pending] = useActionState(createAccount, {});

  const navigateBack = function (): void {
    const returnUrl = getReturnUrl();
    router.replace(returnUrl);
  };

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
    <Stack spacing={3} sx={{ maxWidth: 720 }}>
      <Typography variant="h4">Create Account</Typography>
      <Paper
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 3,
          p: { xs: 2.5, md: 3 },
        }}
      >
        <Stack spacing={3}>
          <Stack spacing={2.5}>
            <Typography variant="overline" color="text.secondary">
              Account Details
            </Typography>
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
            <Typography variant="overline" color="text.secondary">
              Opening Details
            </Typography>
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
          </Stack>
          <Divider flexItem />
          <Stack spacing={2}>
            <Typography variant="body2" color="text.secondary">
              All fields are required before the account can be created.
            </Typography>
            <ErrorAlert
              errorMessage={state.errorTitle ?? null}
              unmappedErrors={state.unmappedErrors ?? null}
            />
            <DialogActions sx={{ px: 0, pb: 0 }}>
              <Button variant="outlined" onClick={navigateBack}>
                Back
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
                      redirectUrl: getReturnUrl(),
                      request,
                    });
                  });
                }}
              >
                Create
              </Button>
            </DialogActions>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  );
};

export default CreateAccountForm;
