"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import { Button, Divider, Stack } from "@mui/material";
import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useState,
} from "react";
import {
  buildCreateRequest,
  getNormalizedDateOpened,
} from "@/accounts/workspace/helpers";
import {
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/helpers";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import CreatableComboBoxEntryField from "@/framework/forms/CreatableComboBoxEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/accounts/workspace/createAccount";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly financialInstitutions: readonly string[];
  readonly open: boolean;
  readonly onClose: () => void;
  readonly redirectUrl: string;
}

/**
 * Displays the create-account dialog for the workspace.
 */
const CreateAccountForm = function ({
  accountingPeriods,
  financialInstitutions,
  open,
  onClose,
  redirectUrl,
}: CreateAccountFormProps): JSX.Element {
  const router = useRouter();
  const [name, setName] = useState<string>("");
  const [financialInstitution, setFinancialInstitution] = useState<
    string | null
  >(null);
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

  useEffect(() => {
    if (state.success === true) {
      onClose();
      router.replace(redirectUrl, { scroll: false });
    }
  }, [onClose, redirectUrl, router, state.success]);

  const request: CreateAccountRequest | null = buildCreateRequest(
    name,
    financialInstitution,
    accountType,
    accountingPeriod,
    dateOpened,
  );

  return (
    <Dialog
      open={open}
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="md"
      title="Create Account"
      actions={
        <>
          <Button disabled={pending} onClick={onClose}>
            Cancel
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
            Create Account
          </Button>
        </>
      }
    >
      <Stack spacing={3}>
        <StringEntryField
          label="Name"
          value={name}
          setValue={setName}
          errorMessage={state.nameErrors ?? null}
        />
        <CreatableComboBoxEntryField
          label="Financial Institution"
          options={financialInstitutions}
          value={financialInstitution}
          setValue={setFinancialInstitution}
        />
        <AccountTypeEntryField
          label="Type"
          value={accountType}
          setValue={setAccountType}
          errorMessage={state.typeErrors ?? null}
        />
        <Divider />
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
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Dialog>
  );
};

export default CreateAccountForm;
