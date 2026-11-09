"use client";

import {
  type JSX,
  startTransition,
  useActionState,
  useEffect,
  useEffectEvent,
  useState,
} from "react";
import { Stack, Typography } from "@mui/material";
import {
  buildCreateRequest,
  buildOnboardRequest,
  getNormalizedDateOpened,
} from "@/accounts/workspace/helpers";
import {
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/helpers";
import AccountGoalFields from "@/account-goals/AccountGoalFields";
import { AccountType } from "@/accounts/types";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import type { AccountingPeriod } from "@/accounting-periods/types";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import ContentSurface from "@/framework/view/ContentSurface";
import CreatableComboBoxEntryField from "@/framework/forms/CreatableComboBoxEntryField";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import FormPage from "@/framework/view/FormPage";
import type { Route } from "next";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/accounts/workspace/createAccount";
import onboardAccount from "@/accounts/workspace/onboardAccount";
import { useRouter } from "next/navigation";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountSetupForm component. Creating an account opens it in one of the open periods; onboarding adds
 * an existing account with its balance before any periods exist.
 */
type AccountSetupFormProps = {
  readonly financialInstitutions: readonly string[];
  readonly redirectUrl: Route;
} & (
  | {
      readonly mode: "create";
      readonly accountingPeriods: readonly AccountingPeriod[];
    }
  | { readonly mode: "onboard" }
);

/**
 * Page for creating or onboarding an account, with goal bounds for a standard account.
 */
const AccountSetupForm = function (props: AccountSetupFormProps): JSX.Element {
  const { financialInstitutions, redirectUrl, mode } = props;
  const canWrite = useWriteAccess();
  const router = useRouter();
  const [name, setName] = useState("");
  const [financialInstitution, setFinancialInstitution] = useState<
    string | null
  >(null);
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(null);
  const [dateOpened, setDateOpened] = useState<Dayjs | null>(
    getDefaultDate(null),
  );
  const [onboardedBalance, setOnboardedBalance] = useState<number | null>(null);
  const [minimumEndingBalance, setMinimumEndingBalance] = useState(0);
  const [maximumEndingBalance, setMaximumEndingBalance] = useState<
    number | null
  >(null);
  const [createState, create, isCreatePending] = useActionState(
    createAccount,
    {},
  );
  const [onboardState, onboard, isOnboardPending] = useActionState(
    onboardAccount,
    {},
  );
  const state = mode === "create" ? createState : onboardState;
  const pending = isCreatePending || isOnboardPending;

  const handleSuccess = useEffectEvent((): void => {
    router.replace(redirectUrl);
  });
  useEffect(() => {
    if (state.success === true) {
      handleSuccess();
    }
  }, [state.success]);

  const goal = { minimumEndingBalance, maximumEndingBalance };
  const createRequest =
    mode === "create"
      ? buildCreateRequest(
          name,
          financialInstitution,
          accountType,
          accountingPeriod,
          dateOpened,
          goal,
        )
      : null;
  const onboardRequest =
    mode === "onboard"
      ? buildOnboardRequest(
          name,
          financialInstitution,
          accountType,
          onboardedBalance,
          goal,
        )
      : null;
  const canSubmit =
    canWrite && (createRequest !== null || onboardRequest !== null);
  const label = mode === "create" ? "Create Account" : "Onboard Account";

  return (
    <FormPage
      title={label}
      returnHref={redirectUrl}
      returnLabel="Back to Accounts"
      submitLabel={label}
      canSubmit={canSubmit}
      pending={pending}
      onSubmit={() => {
        startTransition(() => {
          if (createRequest !== null) {
            create({ redirectUrl, request: createRequest });
          } else if (onboardRequest !== null) {
            onboard({ redirectUrl, request: onboardRequest });
          }
        });
      }}
      errorTitle={state.errorTitle ?? null}
      unmappedErrors={state.unmappedErrors ?? null}
    >
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="h5">Account</Typography>
          <StringEntryField
            label="Name"
            value={name}
            setValue={setName}
            errorMessage={state.nameErrors ?? null}
          />
          <CreatableComboBoxEntryField
            options={financialInstitutions}
            value={financialInstitution}
            setValue={setFinancialInstitution}
          />
          <AccountTypeEntryField
            value={accountType}
            setValue={setAccountType}
            errorMessage={state.typeErrors ?? null}
          />
          {props.mode === "create" ? (
            <>
              <AccountingPeriodEntryField
                label="Opening Accounting Period"
                options={[...props.accountingPeriods]}
                value={accountingPeriod}
                setValue={(period) => {
                  setAccountingPeriod(period);
                  setDateOpened((current) =>
                    getNormalizedDateOpened(period, current),
                  );
                }}
                errorMessage={createState.accountingPeriodErrors ?? null}
              />
              <DateEntryField
                label="Date Opened"
                value={dateOpened}
                setValue={setDateOpened}
                errorMessage={createState.dateOpenedErrors ?? null}
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
            </>
          ) : (
            <CurrencyEntryField
              label="Starting Balance"
              value={onboardedBalance}
              setValue={setOnboardedBalance}
              errorMessage={onboardState.onboardedBalanceErrors ?? null}
            />
          )}
        </Stack>
      </ContentSurface>
      {accountType === AccountType.Standard ? (
        <ContentSurface>
          <Stack spacing={2}>
            <Typography variant="h5">Goal</Typography>
            <AccountGoalFields
              minimumEndingBalance={minimumEndingBalance}
              setMinimumEndingBalance={setMinimumEndingBalance}
              maximumEndingBalance={maximumEndingBalance}
              setMaximumEndingBalance={setMaximumEndingBalance}
              minimumEndingBalanceErrors={
                state.minimumEndingBalanceErrors ?? null
              }
              maximumEndingBalanceErrors={
                state.maximumEndingBalanceErrors ?? null
              }
            />
          </Stack>
        </ContentSurface>
      ) : null}
    </FormPage>
  );
};

export default AccountSetupForm;
