"use client";

import type { AccountType, CreateAccountRequest } from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
} from "@/accounting-periods/types";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
} from "@mui/material";
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
    >
      <DialogTitle>Create Account</DialogTitle>
      <DialogContent>
        <Stack ref={formRef} spacing={3} sx={{ pt: 1 }}>
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
        </Stack>
      </DialogContent>
      <DialogActions>
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
      </DialogActions>
    </Dialog>
  );
};

export default CreateAccountForm;
