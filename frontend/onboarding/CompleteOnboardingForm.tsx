"use client";

import { AddCircleOutline, Delete } from "@mui/icons-material";
import { Button, Paper, Stack, Typography } from "@mui/material";
import { type AccountType, isDebtAccountType } from "@/accounts/types";
import type { components } from "@/framework/data/api";
import formatCurrency from "@/framework/formatCurrency";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import StringEntryField from "@/framework/forms/StringEntryField";
import ColumnButton from "@/framework/listframe/ColumnButton";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import { type JSX, startTransition, useActionState, useState } from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import completeOnboarding from "@/onboarding/completeOnboarding";

type OnboardingRequest = components["schemas"]["OnboardingModel"];
type OnboardingAccount = components["schemas"]["OnboardingAccountModel"];
type OnboardingFund = components["schemas"]["OnboardingFundModel"];

interface DraftOnboardingAccount {
  readonly name: string;
  readonly type: AccountType | null;
  readonly balance: number | null;
}

interface DraftOnboardingFund {
  readonly name: string;
  readonly description: string;
  readonly balance: number | null;
}

interface CompleteOnboardingAccount extends DraftOnboardingAccount {
  readonly type: AccountType;
  readonly balance: number;
}

interface CompleteOnboardingFund extends DraftOnboardingFund {
  readonly balance: number;
}

const createEmptyAccount = function (): DraftOnboardingAccount {
  return {
    name: "",
    type: null,
    balance: null,
  };
};

const createEmptyFund = function (): DraftOnboardingFund {
  return {
    name: "",
    description: "",
    balance: null,
  };
};

const isAccountComplete = function (
  account: DraftOnboardingAccount,
): account is CompleteOnboardingAccount {
  return (
    account.name.trim() !== "" &&
    account.type !== null &&
    account.balance !== null
  );
};

const isFundComplete = function (
  fund: DraftOnboardingFund,
): fund is CompleteOnboardingFund {
  return fund.name.trim() !== "" && fund.balance !== null;
};

const getSignedAccountBalance = function (
  account: DraftOnboardingAccount,
): number {
  if (account.type === null || account.balance === null) {
    return 0;
  }

  return isDebtAccountType(account.type) ? -account.balance : account.balance;
};

const toOnboardingAccount = function (
  account: CompleteOnboardingAccount,
): OnboardingAccount {
  return {
    name: account.name.trim(),
    type: account.type,
    balance: account.balance,
  };
};

const toOnboardingFund = function (
  fund: CompleteOnboardingFund,
): OnboardingFund {
  return {
    name: fund.name.trim(),
    description: fund.description.trim(),
    balance: fund.balance,
  };
};

/**
 * Displays an editable onboarding account row.
 */
const OnboardingAccountRow = function ({
  index,
  value,
  setValue,
  onDelete,
}: {
  readonly index: number;
  readonly value: DraftOnboardingAccount;
  readonly setValue: (newValue: DraftOnboardingAccount) => void;
  readonly onDelete: () => void;
}): JSX.Element {
  return (
    <Paper variant="outlined" sx={{ padding: 2 }}>
      <Stack direction="row" spacing={1} alignItems="flex-start">
        <Stack spacing={2} sx={{ flex: 1 }}>
          <Typography variant="caption" color="text.secondary">
            Account {index + 1}
          </Typography>
          <StringEntryField
            label="Name"
            value={value.name}
            setValue={(name): void => {
              setValue({ ...value, name });
            }}
          />
          <AccountTypeEntryField
            label="Type"
            value={value.type}
            setValue={(type): void => {
              setValue({ ...value, type });
            }}
          />
          <CurrencyEntryField
            label="Opening Balance"
            value={value.balance}
            setValue={(balance): void => {
              setValue({ ...value, balance });
            }}
          />
        </Stack>
        <ColumnButton
          label="Delete account"
          icon={<Delete />}
          onClick={onDelete}
        />
      </Stack>
    </Paper>
  );
};

/**
 * Displays an editable onboarding fund row.
 */
const OnboardingFundRow = function ({
  index,
  value,
  setValue,
  onDelete,
}: {
  readonly index: number;
  readonly value: DraftOnboardingFund;
  readonly setValue: (newValue: DraftOnboardingFund) => void;
  readonly onDelete: () => void;
}): JSX.Element {
  return (
    <Paper variant="outlined" sx={{ padding: 2 }}>
      <Stack direction="row" spacing={1} alignItems="flex-start">
        <Stack spacing={2} sx={{ flex: 1 }}>
          <Typography variant="caption" color="text.secondary">
            Fund {index + 1}
          </Typography>
          <StringEntryField
            label="Name"
            value={value.name}
            setValue={(name): void => {
              setValue({ ...value, name });
            }}
          />
          <StringEntryField
            label="Description"
            value={value.description}
            setValue={(description): void => {
              setValue({ ...value, description });
            }}
          />
          <CurrencyEntryField
            label="Starting Balance"
            value={value.balance}
            setValue={(balance): void => {
              setValue({ ...value, balance });
            }}
          />
        </Stack>
        <ColumnButton
          label="Delete fund"
          icon={<Delete />}
          onClick={onDelete}
        />
      </Stack>
    </Paper>
  );
};

/**
 * Displays the action that completes the captive onboarding flow.
 */
const CompleteOnboardingForm = function (): JSX.Element {
  const [accounts, setAccounts] = useState<DraftOnboardingAccount[]>([]);
  const [funds, setFunds] = useState<DraftOnboardingFund[]>([]);
  const [state, action, pending] = useActionState(completeOnboarding, {});

  const totalAccountBalance = accounts.reduce(
    (sum, account) => sum + getSignedAccountBalance(account),
    0,
  );
  const totalFundBalance = funds.reduce(
    (sum, fund) => sum + (fund.balance ?? 0),
    0,
  );
  const unassignedBalance = totalAccountBalance - totalFundBalance;
  const hasIncompleteAccounts = accounts.some(
    (account) => !isAccountComplete(account),
  );
  const hasIncompleteFunds = funds.some((fund) => !isFundComplete(fund));
  const hasOverAssignedFunds = unassignedBalance < 0;

  let request: OnboardingRequest | null = null;
  const completeAccounts = accounts.filter(isAccountComplete);
  const completeFunds = funds.filter(isFundComplete);
  if (
    completeAccounts.length === accounts.length &&
    completeFunds.length === funds.length &&
    !hasOverAssignedFunds
  ) {
    request = {
      accounts: completeAccounts.map(toOnboardingAccount),
      funds: completeFunds.map(toOnboardingFund),
    };
  }

  return (
    <Paper
      elevation={3}
      sx={{
        maxWidth: 1200,
        padding: { xs: 3, md: 4 },
        width: "100%",
      }}
    >
      <Stack spacing={3}>
        <Stack spacing={1}>
          <Typography variant="h4">Set Up Financial Tracker</Typography>
          <Typography color="text.secondary">
            This workspace is still in its first-run state. Complete onboarding
            before accessing accounts, funds, transactions, or accounting
            periods.
          </Typography>
        </Stack>
        <Typography color="text.secondary">
          Add the accounts you already have and optionally assign opening fund
          balances. Any remaining net balance becomes the automatically created
          Unassigned fund.
        </Typography>
        <Stack
          direction={{ xs: "column", lg: "row" }}
          spacing={3}
          alignItems="stretch"
        >
          <Stack sx={{ flex: 1, minWidth: 0 }}>
            <CaptionedFrame caption="Accounts" minWidth={0} maxWidth={null}>
              <Stack spacing={2} sx={{ marginTop: "16px" }}>
                {accounts.length === 0 ? (
                  <Typography color="text.secondary" variant="body2">
                    No accounts added yet.
                  </Typography>
                ) : null}
                {accounts.map((account, index) => (
                  <OnboardingAccountRow
                    key={index}
                    index={index}
                    value={account}
                    setValue={(newValue): void => {
                      setAccounts((currentAccounts) =>
                        currentAccounts.map((currentAccount, currentIndex) =>
                          currentIndex === index ? newValue : currentAccount,
                        ),
                      );
                    }}
                    onDelete={() => {
                      setAccounts((currentAccounts) =>
                        currentAccounts.filter(
                          (_, currentIndex) => currentIndex !== index,
                        ),
                      );
                    }}
                  />
                ))}
                <Button
                  variant="contained"
                  startIcon={<AddCircleOutline />}
                  disableElevation
                  onClick={() => {
                    setAccounts((currentAccounts) => [
                      ...currentAccounts,
                      createEmptyAccount(),
                    ]);
                  }}
                >
                  Add Account
                </Button>
                <Stack spacing={0.5}>
                  <Typography variant="body2">
                    Net Opening Balance: {formatCurrency(totalAccountBalance)}
                  </Typography>
                  <Typography color="text.secondary" variant="caption">
                    Debt and credit card accounts reduce the amount available to
                    assign to funds.
                  </Typography>
                </Stack>
              </Stack>
            </CaptionedFrame>
          </Stack>
          <Stack sx={{ flex: 1, minWidth: 0 }}>
            <CaptionedFrame caption="Funds" minWidth={0} maxWidth={null}>
              <Stack spacing={2} sx={{ marginTop: "16px" }}>
                {funds.length === 0 ? (
                  <Typography color="text.secondary" variant="body2">
                    No funds added yet.
                  </Typography>
                ) : null}
                {funds.map((fund, index) => (
                  <OnboardingFundRow
                    key={index}
                    index={index}
                    value={fund}
                    setValue={(newValue): void => {
                      setFunds((currentFunds) =>
                        currentFunds.map((currentFund, currentIndex) =>
                          currentIndex === index ? newValue : currentFund,
                        ),
                      );
                    }}
                    onDelete={() => {
                      setFunds((currentFunds) =>
                        currentFunds.filter(
                          (_, currentIndex) => currentIndex !== index,
                        ),
                      );
                    }}
                  />
                ))}
                <Button
                  variant="contained"
                  startIcon={<AddCircleOutline />}
                  disableElevation
                  onClick={() => {
                    setFunds((currentFunds) => [
                      ...currentFunds,
                      createEmptyFund(),
                    ]);
                  }}
                >
                  Add Fund
                </Button>
                <Stack spacing={0.5}>
                  <Typography variant="body2">
                    Assigned to Funds: {formatCurrency(totalFundBalance)}
                  </Typography>
                  <Typography
                    color={hasOverAssignedFunds ? "error" : "text.primary"}
                    variant="body2"
                  >
                    {hasOverAssignedFunds
                      ? `Over Assigned By: ${formatCurrency(Math.abs(unassignedBalance))}`
                      : `Unassigned Amount: ${formatCurrency(unassignedBalance)}`}
                  </Typography>
                  <Typography color="text.secondary" variant="caption">
                    The remaining balance is created automatically as the
                    Unassigned fund during onboarding.
                  </Typography>
                </Stack>
              </Stack>
            </CaptionedFrame>
          </Stack>
        </Stack>
        <Stack spacing={1}>
          {hasIncompleteAccounts || hasIncompleteFunds ? (
            <Typography color="error" variant="caption">
              Complete or remove unfinished account and fund rows before
              finishing onboarding.
            </Typography>
          ) : null}
          {hasOverAssignedFunds ? (
            <Typography color="error" variant="caption">
              Fund balances cannot exceed the net opening balance of your
              accounts.
            </Typography>
          ) : null}
        </Stack>
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
          Complete Onboarding
        </Button>
        <ErrorAlert
          errorMessage={state.errorTitle ?? null}
          unmappedErrors={state.unmappedErrors ?? null}
        />
      </Stack>
    </Paper>
  );
};

export default CompleteOnboardingForm;
