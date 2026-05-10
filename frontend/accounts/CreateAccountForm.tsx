"use client";

import {
  type AccountType,
  type CreateAccountRequest,
  isDebtAccountType,
  isTrackedAccountType,
} from "@/accounts/types";
import {
  type AccountingPeriod,
  getDefaultDate,
  getMaximumDate,
  getMinimumDate,
} from "@/accounting-periods/types";
import { Button, DialogActions, Stack, Typography } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import FundAssignmentEntryFrame, {
  updateUnassignedFundAmount,
} from "@/funds/FundAssignmentEntryFrame";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  isIncomeTransactionComplete,
  isSpendingTransactionComplete,
} from "@/transactions/types";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import AccountingPeriodEntryField from "@/accounting-periods/AccountingPeriodEntryField";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
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
  readonly funds: Fund[];
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
  routeAccountingPeriod = null,
}: CreateAccountFormProps): JSX.Element {
  const [name, setName] = useState<string>("");
  const [accountType, setAccountType] = useState<AccountType | null>(null);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(routeAccountingPeriod);
  const [addDate, setAddDate] = useState<Dayjs | null>(
    getDefaultDate(routeAccountingPeriod),
  );
  const [initialBalance, setInitialBalance] = useState<number | null>(null);
  const [initialFundAssignments, setInitialFundAssignments] = useState<
    FundAmount[]
  >([]);

  const [state, action, pending] = useActionState(createAccount, {
    redirectUrl: getRedirectUrl(routeAccountingPeriod),
  });

  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;
  const trackedAccountType =
    accountType !== null && isTrackedAccountType(accountType);

  const onAccountTypeChange = function (
    newAccountType: AccountType | null,
  ): void {
    setAccountType(newAccountType);
    if (newAccountType === null || !isTrackedAccountType(newAccountType)) {
      setInitialFundAssignments([]);
    } else {
      updateUnassignedFundAmount(
        unassignedFund,
        initialBalance,
        initialFundAssignments,
      );
    }
  };

  const onAccountingPeriodChange = function (
    newAccountingPeriod: AccountingPeriod | null,
  ): void {
    setAccountingPeriod(newAccountingPeriod);
    setAddDate((currentAddDate) =>
      getNormalizedAddDate(newAccountingPeriod, currentAddDate),
    );
  };

  const onInitialBalanceChange = function (
    newInitialBalance: number | null,
  ): void {
    setInitialBalance(newInitialBalance);
    if (trackedAccountType) {
      setInitialFundAssignments((currentValue) =>
        updateUnassignedFundAmount(
          unassignedFund,
          newInitialBalance,
          currentValue,
        ),
      );
    }
  };

  let request: CreateAccountRequest | null = null;
  if (
    name !== "" &&
    accountType !== null &&
    accountingPeriod !== null &&
    addDate !== null &&
    initialBalance !== null &&
    (!trackedAccountType ||
      (isDebtAccountType(accountType) &&
        isSpendingTransactionComplete(initialFundAssignments)) ||
      (!isDebtAccountType(accountType) &&
        isIncomeTransactionComplete(initialFundAssignments)))
  ) {
    request = {
      name,
      type: accountType,
      accountingPeriodId: accountingPeriod.id,
      addDate: addDate.format("YYYY-MM-DD"),
      initialBalance,
      initialFundAssignments: trackedAccountType
        ? initialFundAssignments
            .filter(
              (fundAmount) =>
                fundAmount.fundId !== "" && fundAmount.fundName !== "",
            )
            .map((fundAmount) => ({
              fundId: fundAmount.fundId,
              amount: fundAmount.amount,
            }))
        : [],
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
          setValue={onAccountTypeChange}
          errorMessage={state.typeErrors ?? null}
        />
        <AccountingPeriodEntryField
          label="Accounting Period"
          options={accountingPeriods}
          value={accountingPeriod}
          setValue={
            routeAccountingPeriod === null ? onAccountingPeriodChange : null
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
          setValue={onInitialBalanceChange}
          errorMessage={state.initialBalanceErrors ?? null}
        />
        {trackedAccountType ? (
          <>
            <FundAssignmentEntryFrame
              label="Initial Fund Assignments"
              funds={funds}
              totalAmountToAssign={initialBalance}
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
