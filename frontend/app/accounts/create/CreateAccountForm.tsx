"use client";

import {
  type AccountType,
  type CreateAccountRequest,
  isTrackedAccountType,
} from "@/data/accountTypes";
import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/data/accountingPeriodTypes";
import { Button, DialogActions, Stack, Typography } from "@mui/material";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, startTransition, useActionState, useState } from "react";
import routes, { routeBreadcrumbs } from "@/framework/routes";
import AccountTypeEntryField from "@/framework/forms/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/framework/forms/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import Link from "next/link";
import StringEntryField from "@/framework/forms/StringEntryField";
import createAccount from "@/app/accounts/create/createAccount";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the CreateAccountForm component.
 */
interface CreateAccountFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: FundIdentifier[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
}

/**
 * Gets the URL to redirect the user to after successfully creating an account.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
): string {
  if (providedAccountingPeriod !== null) {
    return routes.accountingPeriods.detail(providedAccountingPeriod.id);
  }
  return routes.accounts.index;
};

/**
 * Determines whether any initial fund assignments are incomplete.
 */
const hasIncompleteFundAssignments = function (
  initialFundAssignments: FundAmount[],
): boolean {
  return initialFundAssignments.some(
    (fundAmount) => fundAmount.fundId === "" || fundAmount.fundName === "",
  );
};

/**
 * Normalizes the add date for the selected accounting period.
 */
const getNormalizedAddDate = function (
  accountingPeriod: AccountingPeriod | null,
  addDate: Dayjs | null,
): Dayjs | null {
  if (accountingPeriod === null) {
    return null;
  }

  const minimumDate = getMinimumDate(accountingPeriod);
  const maximumDate = getMaximumDate(accountingPeriod);
  if (
    addDate === null ||
    addDate.isBefore(minimumDate) ||
    addDate.isAfter(maximumDate)
  ) {
    return getDefaultDate(accountingPeriod);
  }

  return addDate;
};

/**
 * Component that displays the form for creating an account.
 */
const CreateAccountForm = function ({
  accountingPeriods,
  funds,
  providedAccountingPeriod = null,
}: CreateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod);
  const [addDate, setAddDate] = useState<Dayjs | null>(
    getDefaultDate(providedAccountingPeriod),
  );
  const [initialBalance, setInitialBalance] = useState<number | null>(null);
  const [initialFundAssignments, setInitialFundAssignments] = useState<
    FundAmount[]
  >([]);

  const [state, action, pending] = useActionState(createAccount, {
    redirectUrl: getRedirectUrl(providedAccountingPeriod),
  });

  const trackedAccountType =
    accountType !== null && isTrackedAccountType(accountType);
  const totalAssigned = initialFundAssignments.reduce(
    (sum, fundAmount) => sum + fundAmount.amount,
    0,
  );

  const handleAccountTypeChange = function (
    newAccountType: AccountType | null,
  ): void {
    setAccountType(newAccountType);
    if (newAccountType === null || !isTrackedAccountType(newAccountType)) {
      setInitialFundAssignments([]);
    }
  };

  const handleAccountingPeriodChange = function (
    newAccountingPeriod: AccountingPeriod | null,
  ): void {
    setAccountingPeriod(newAccountingPeriod);
    setAddDate((currentAddDate) =>
      getNormalizedAddDate(newAccountingPeriod, currentAddDate),
    );
  };

  const canSubmit =
    name !== "" &&
    accountType !== null &&
    accountingPeriod !== null &&
    addDate !== null &&
    initialBalance !== null &&
    (!trackedAccountType ||
      !hasIncompleteFundAssignments(initialFundAssignments));

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={routeBreadcrumbs.accounts.create(providedAccountingPeriod)}
      />
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
          setValue={handleAccountTypeChange}
          errorMessage={state.typeErrors ?? null}
        />
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            providedAccountingPeriod === null
              ? handleAccountingPeriodChange
              : null
          }
          errorMessage={state.accountingPeriodErrors ?? null}
        />
        <DateEntryField
          label="Add Date"
          value={addDate}
          setValue={setAddDate}
          errorMessage={state.addDateErrors ?? null}
          minDate={
            accountingPeriod === null ? null : getMinimumDate(accountingPeriod)
          }
          maxDate={
            accountingPeriod === null ? null : getMaximumDate(accountingPeriod)
          }
          disabled={accountingPeriod === null}
        />
        <CurrencyEntryField
          label="Initial Balance"
          value={initialBalance}
          setValue={setInitialBalance}
          errorMessage={state.initialBalanceErrors ?? null}
        />
        {trackedAccountType ? (
          <>
            <FundAmountCollectionEntryFrame
              label={`Initial Fund Assignments (Total Assigned: ${formatCurrency(totalAssigned)})`}
              funds={funds}
              value={initialFundAssignments}
              setValue={setInitialFundAssignments}
            />
            {state.initialFundAssignmentsErrors !== null &&
            typeof state.initialFundAssignmentsErrors !== "undefined" ? (
              <Typography color="error" variant="caption">
                {state.initialFundAssignmentsErrors}
              </Typography>
            ) : null}
          </>
        ) : null}
        <DialogActions>
          <Link href={getRedirectUrl(providedAccountingPeriod)} tabIndex={-1}>
            <Button variant="outlined">Cancel</Button>
          </Link>
          <Button
            variant="contained"
            loading={pending}
            disabled={!canSubmit}
            onClick={() => {
              if (
                name === "" ||
                accountType === null ||
                accountingPeriod === null ||
                addDate === null ||
                initialBalance === null
              ) {
                return;
              }

              const request: CreateAccountRequest = {
                name,
                type: accountType,
                accountingPeriodId: accountingPeriod.id,
                addDate: addDate.format("YYYY-MM-DD"),
                initialBalance,
                initialFundAssignments: trackedAccountType
                  ? initialFundAssignments
                      .filter(
                        (fundAmount) =>
                          fundAmount.fundId !== "" &&
                          fundAmount.fundName !== "",
                      )
                      .map((fundAmount) => ({
                        fundId: fundAmount.fundId,
                        amount: fundAmount.amount,
                      }))
                  : [],
              };

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
