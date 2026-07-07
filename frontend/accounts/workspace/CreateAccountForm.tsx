"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
} from "@/accounting-periods/types";
import { Button, Stack, Typography } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import {
  buildCreateRequest,
  getNormalizedDateOpened,
} from "@/accounts/workspace/helpers";
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import AccountOpeningFrame from "@/accounts/workspace/AccountOpeningFrame";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import createAccount from "@/accounts/workspace/createAccount";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly redirectUrl: string;
}

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
  const formRef = useRef<HTMLDivElement | null>(null);

  const [state, action, pending] = useActionState(createAccount, {});
  const detailsAreValid = name !== "" && accountType !== null;
  const openingIsValid = accountingPeriod !== null && dateOpened !== null;

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
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      reset();
    }
  }, [state]);

  const request: CreateAccountRequest | null = buildCreateRequest(
    name,
    accountType,
    accountingPeriod,
    dateOpened,
  );

  return (
    <Stack ref={formRef} spacing={3} sx={{ width: "100%", maxWidth: 1200 }}>
      <Typography variant="h5">Create Account</Typography>
      <AccountDetailsFrame
        color={detailsAreValid ? "info" : "error"}
        name={name}
        setName={setName}
        nameErrorMessage={state.nameErrors ?? null}
        accountType={accountType}
        setAccountType={setAccountType}
        accountTypeErrorMessage={state.typeErrors ?? null}
      />
      <AccountOpeningFrame
        accountingPeriods={accountingPeriods}
        accountingPeriod={accountingPeriod}
        setAccountingPeriod={onAccountingPeriodChange}
        accountingPeriodErrorMessage={state.accountingPeriodErrors ?? null}
        dateOpened={dateOpened}
        setDateOpened={setDateOpened}
        dateOpenedErrorMessage={state.dateOpenedErrors ?? null}
        color={openingIsValid ? "info" : "error"}
      />
      <ErrorAlert
        errorMessage={state.errorTitle ?? null}
        unmappedErrors={state.unmappedErrors ?? null}
      />
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={1.5}
        justifyContent="flex-end"
      >
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
      </Stack>
    </Stack>
  );
};

export default CreateAccountForm;
