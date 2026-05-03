"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import {
  CreateAccountTransactionType,
  CreateFundTransactionType,
  CreateIncomeTransactionType,
  CreateSpendingTransactionType,
  type CreateTransactionRequest,
  isAccountTransaction,
  isFundTransaction,
  isFundTransactionComplete,
  isIncomeTransaction,
  isIncomeTransactionComplete,
  isSpendingTransaction,
  isSpendingTransactionComplete,
} from "@/transactions/types";
import type { Fund, FundAmount } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Breadcrumbs from "@/framework/Breadcrumbs";
import CreateOrUpdateIncomeTransactionFrame from "@/transactions/CreateOrUpdateIncomeTransactionFrame";
import CreateOrUpdateSpendingTransactionFrame from "@/transactions/CreateOrUpdateSpendingTransactionFrame";
import CreateOrUpdateTransactionDetailsFrame from "@/transactions/CreateOrUpdateTransactionDetailsFrame";
import CreateOrUpdateTransactionFromToFrame from "@/transactions/CreateOrUpdateTransactionFromToFrame";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import { ToggleState } from "@/accounting-periods/AccountingPeriodViewListFrames";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import breadcrumbs from "@/transactions/breadcrumbs";
import createTransaction from "@/transactions/createTransaction";
import fundRoutes from "@/funds/routes";
import { updateUnassignedFundAmount } from "@/funds/FundAssignmentEntryFrame";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly providedAccountingPeriod?: AccountingPeriod | null;
  readonly providedDebitAccount?: Account | null;
  readonly providedCreditAccount?: Account | null;
  readonly providedDebitFund?: Fund | null;
  readonly providedCreditFund?: Fund | null;
}

/**
 * Gets the URL to redirect the user to after successfully creating a transaction.
 */
const getRedirectUrl = function (
  providedAccountingPeriod: AccountingPeriod | null,
  providedAccount: Account | null,
  providedFund: Fund | null,
): string {
  if (providedAccountingPeriod !== null) {
    if (providedAccount !== null) {
      return accountingPeriodRoutes.accountDetail({
        id: providedAccountingPeriod.id,
        accountId: providedAccount.id,
      });
    }
    if (providedFund !== null) {
      return accountingPeriodRoutes.fundDetail(
        {
          id: providedAccountingPeriod.id,
          fundId: providedFund.id,
        },
        {},
      );
    }
    return accountingPeriodRoutes.detail(
      { id: providedAccountingPeriod.id },
      { display: ToggleState.Transactions },
    );
  }
  if (providedAccount !== null) {
    return accountRoutes.detail({ id: providedAccount.id }, {});
  }
  if (providedFund !== null) {
    return fundRoutes.detail({ id: providedFund.id }, {});
  }
  return "";
};

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  providedAccountingPeriod,
  providedDebitAccount,
  providedCreditAccount,
  providedDebitFund,
  providedCreditFund,
}: CreateTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;

  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(providedAccountingPeriod ?? null);
  const [date, setDate] = useState<Dayjs | null>(null);
  const defaultDate =
    accountingPeriod !== null
      ? dayjs(`${accountingPeriod.year}-${accountingPeriod.month}-01`)
      : null;
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);

  const [debitAccount, setDebitAccount] = useState<Account | null>(
    providedDebitAccount ?? null,
  );
  const [creditAccount, setCreditAccount] = useState<Account | null>(
    providedCreditAccount ?? null,
  );
  const [debitFund, setDebitFund] = useState<Fund | null>(
    providedDebitFund ?? null,
  );
  const [creditFund, setCreditFund] = useState<Fund | null>(
    providedCreditFund ?? null,
  );

  const [incomeFundAssignments, setIncomeFundAssignments] = useState<
    FundAmount[]
  >([]);

  const [spendingFundAssignments, setSpendingFundAssignments] = useState<
    FundAmount[]
  >([]);

  /**
   * Event handler for when the from or to fields are changed in the create transaction form.
   */
  const onToFromChange = function (
    newDebitAccount: Account | null,
    newCreditAccount: Account | null,
    newDebitFund: Fund | null,
    newCreditFund: Fund | null,
  ): void {
    if (newDebitAccount !== null) {
      setDebitAccount(newDebitAccount);
      setDebitFund(null);
    } else if (newCreditAccount !== null) {
      setCreditAccount(newCreditAccount);
      setCreditFund(null);
    } else if (newDebitFund !== null) {
      setDebitFund(newDebitFund);
      setDebitAccount(null);
    } else if (newCreditFund !== null) {
      setCreditFund(newCreditFund);
      setCreditAccount(null);
    }

    if (
      !isIncomeTransaction(
        newDebitAccount,
        newCreditAccount,
        newDebitFund,
        newCreditFund,
      )
    ) {
      setIncomeFundAssignments([]);
    } else {
      updateUnassignedFundAmount(unassignedFund, amount, incomeFundAssignments);
    }
    if (
      !isSpendingTransaction(
        newDebitAccount,
        newCreditAccount,
        newDebitFund,
        newCreditFund,
      )
    ) {
      setSpendingFundAssignments([]);
    } else {
      updateUnassignedFundAmount(
        unassignedFund,
        amount,
        spendingFundAssignments,
      );
    }
  };

  /**
   * Event handler for when the amount field is changed in the create transaction form.
   */
  const onAmountChange = function (newAmount: number | null): void {
    setAmount(newAmount);

    updateUnassignedFundAmount(
      unassignedFund,
      newAmount,
      incomeFundAssignments,
    );
    updateUnassignedFundAmount(
      unassignedFund,
      newAmount,
      spendingFundAssignments,
    );
  };

  let request: CreateTransactionRequest | null = null;
  if (
    accountingPeriod !== null &&
    (date !== null || defaultDate !== null) &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0
  ) {
    if (
      isIncomeTransaction(debitAccount, creditAccount, debitFund, creditFund) &&
      isIncomeTransactionComplete(incomeFundAssignments)
    ) {
      request = {
        type: CreateIncomeTransactionType.Income,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("yyyy-MM-DD") ?? defaultDate?.format("yyyy-MM-DD") ?? "",
        location,
        description,
        amount,
        debitAccount:
          debitAccount !== null
            ? {
                accountId: debitAccount.id,
              }
            : null,
        creditAccount: {
          accountId: creditAccount?.id ?? "",
        },
        fundAssignments: incomeFundAssignments.map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
      };
    } else if (
      isSpendingTransaction(
        debitAccount,
        creditAccount,
        debitFund,
        creditFund,
      ) &&
      isSpendingTransactionComplete(spendingFundAssignments)
    ) {
      request = {
        type: CreateSpendingTransactionType.Spending,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("yyyy-MM-DD") ?? defaultDate?.format("yyyy-MM-DD") ?? "",
        location,
        description,
        amount,
        debitAccount: {
          accountId: debitAccount?.id ?? "",
        },
        creditAccount:
          creditAccount !== null
            ? {
                accountId: creditAccount.id,
              }
            : null,
        fundAssignments: spendingFundAssignments.map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
      };
    } else if (
      isAccountTransaction(debitAccount, creditAccount, debitFund, creditFund)
    ) {
      request = {
        type: CreateAccountTransactionType.Account,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("yyyy-MM-DD") ?? defaultDate?.format("yyyy-MM-DD") ?? "",
        location,
        description,
        amount,
        debitAccount:
          debitAccount !== null
            ? {
                accountId: debitAccount.id,
              }
            : null,
        creditAccount:
          creditAccount !== null
            ? {
                accountId: creditAccount.id,
              }
            : null,
      };
    } else if (
      isFundTransaction(debitAccount, creditAccount, debitFund, creditFund) &&
      isFundTransactionComplete(debitFund, creditFund)
    ) {
      request = {
        type: CreateFundTransactionType.Fund,
        accountingPeriodId: accountingPeriod.id,
        date:
          date?.format("yyyy-MM-DD") ?? defaultDate?.format("yyyy-MM-DD") ?? "",
        location,
        description,
        amount,
        debitFundId: debitFund?.id ?? "",
        creditFundId: creditFund?.id ?? "",
      };
    }
  }

  const [state, action, pending] = useActionState(createTransaction, {
    redirectUrl: getRedirectUrl(
      providedAccountingPeriod ?? null,
      providedDebitAccount ?? providedCreditAccount ?? null,
      providedDebitFund ?? providedCreditFund ?? null,
    ),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.create(
          providedAccountingPeriod ?? null,
          providedDebitAccount ?? providedCreditAccount ?? null,
          providedDebitFund ?? providedCreditFund ?? null,
        )}
      />
      <CreateOrUpdateTransactionDetailsFrame
        accountingPeriods={accountingPeriods}
        accountingPeriod={accountingPeriod}
        setAccountingPeriod={setAccountingPeriod}
        date={date ?? defaultDate}
        setDate={setDate}
        location={location}
        setLocation={setLocation}
        description={description}
        setDescription={setDescription}
        amount={amount}
        setAmount={onAmountChange}
      />
      <CreateOrUpdateTransactionFromToFrame
        accounts={accounts}
        debitAccount={debitAccount}
        setDebitAccount={(newValue): void => {
          onToFromChange(newValue, null, null, null);
        }}
        creditAccount={creditAccount}
        setCreditAccount={(newValue): void => {
          onToFromChange(null, newValue, null, null);
        }}
        funds={funds}
        debitFund={debitFund}
        setDebitFund={(newValue): void => {
          onToFromChange(null, null, newValue, null);
        }}
        creditFund={creditFund}
        setCreditFund={(newValue): void => {
          onToFromChange(null, null, null, newValue);
        }}
      />
      {isIncomeTransaction(
        debitAccount,
        creditAccount,
        debitFund,
        creditFund,
      ) && (
        <CreateOrUpdateIncomeTransactionFrame
          funds={funds}
          amount={amount}
          incomeFundAssignments={incomeFundAssignments}
          setIncomeFundAssignments={setIncomeFundAssignments}
        />
      )}
      {isSpendingTransaction(
        debitAccount,
        creditAccount,
        debitFund,
        creditFund,
      ) && (
        <CreateOrUpdateSpendingTransactionFrame
          funds={funds}
          amount={amount}
          spendingFundAssignments={spendingFundAssignments}
          setSpendingFundAssignments={setSpendingFundAssignments}
        />
      )}
      <DialogActions>
        <Link href={state.redirectUrl} tabIndex={-1}>
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
  );
};

export default CreateTransactionForm;
