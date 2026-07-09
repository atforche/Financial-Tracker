"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import { Box, Button, Stack } from "@mui/material";
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
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Frame from "@/framework/view/Frame";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/accounts/workspace/createAccount";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Displays the create-account dialog for the workspace.
 */
const CreateAccountForm = function ({
  accountingPeriods,
  open,
  onClose,
  redirectUrl,
}: CreateAccountFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const [dateOpened, setDateOpened] = useState<Dayjs | null>(
    getDefaultDate(null),
  );
  const formRef = useRef<HTMLDivElement | null>(null);

  const [state, action, pending] = useActionState(createAccount, {});
  const setupIsValid =
    name !== "" &&
    accountType !== null &&
    accountingPeriod !== null &&
    dateOpened !== null;

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
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  const request: CreateAccountRequest | null = buildCreateRequest(
    name,
    accountType,
    accountingPeriod,
    dateOpened,
  );

  return (
    <Dialog
      open={open}
      onClose={
        pending
          ? // eslint-disable-next-line no-undefined
            undefined
          : (): void => {
              onClose();
              reset();
            }
      }
      fullWidth
      maxWidth="md"
      title="Create Account"
      actions={
        <>
          <Button
            disabled={pending}
            onClick={() => {
              onClose();
              reset();
            }}
          >
            Cancel
          </Button>
          <Button variant="outlined" disabled={pending} onClick={reset}>
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
        </>
      }
    >
      <Stack ref={formRef} spacing={3}>
        <Frame title="Account Setup" color={setupIsValid ? "info" : "error"}>
          <Box
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns: "repeat(auto-fit, minmax(min(100%, 220px)))",
            }}
          >
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
          </Box>
        </Frame>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default CreateAccountForm;
