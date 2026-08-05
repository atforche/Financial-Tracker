"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import { Button, Stack } from "@mui/material";
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
import AccountDetailsFrame from "@/accounts/workspace/AccountDetailsFrame";
import AccountOpeningFrame from "@/accounts/workspace/AccountOpeningFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Dayjs } from "dayjs";
import Dialog from "@/framework/dialog/Dialog";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import createAccount from "@/accounts/workspace/createAccount";
import { getDefaultDate } from "@/accounting-periods/helpers";
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
        <AccountDetailsFrame
          color={name !== "" && accountType !== null ? "info" : "error"}
          name={name}
          setName={setName}
          nameErrorMessage={state.nameErrors ?? null}
          financialInstitution={financialInstitution}
          financialInstitutions={financialInstitutions}
          setFinancialInstitution={setFinancialInstitution}
          accountType={accountType}
          setAccountType={setAccountType}
          accountTypeErrorMessage={state.typeErrors ?? null}
        />
        <AccountOpeningFrame
          color={request === null ? "error" : "info"}
          accountingPeriods={accountingPeriods}
          accountingPeriod={accountingPeriod}
          setAccountingPeriod={onAccountingPeriodChange}
          accountingPeriodErrorMessage={state.accountingPeriodErrors ?? null}
          dateOpened={dateOpened}
          setDateOpened={setDateOpened}
          dateOpenedErrorMessage={state.dateOpenedErrors ?? null}
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
