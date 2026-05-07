"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  type Transaction,
  type UpdateTransactionRequest,
  isIncomeTransactionComplete,
  isSpendingTransactionComplete,
} from "@/transactions/types";
import {
  TransactionModelAccountTransactionModelType,
  TransactionModelFundTransactionModelType,
  TransactionModelIncomeTransactionModelType,
  TransactionModelSpendingTransactionModelType,
  UpdateTransactionModelUpdateAccountTransactionModelType,
  UpdateTransactionModelUpdateFundTransactionModelType,
  UpdateTransactionModelUpdateIncomeTransactionModelType,
  UpdateTransactionModelUpdateSpendingTransactionModelType,
} from "@/framework/data/api";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateIncomeTransactionFrame from "@/transactions/CreateOrUpdateIncomeTransactionFrame";
import CreateOrUpdateSpendingTransactionFrame from "@/transactions/CreateOrUpdateSpendingTransactionFrame";
import CreateOrUpdateTransactionDetailsFrame from "@/transactions/CreateOrUpdateTransactionDetailsFrame";
import CreateOrUpdateTransactionFromToFrame from "@/transactions/CreateOrUpdateTransactionFromToFrame";
import type { Dayjs } from "dayjs";
import ErrorAlert from "@/framework/alerts/ErrorAlert";
import Link from "next/link";
import { ToggleState } from "@/accounting-periods/AccountingPeriodViewListFrames";
import accountRoutes from "@/accounts/routes";
import accountingPeriodRoutes from "@/accounting-periods/routes";
import fundRoutes from "@/funds/routes";
import updateTransaction from "@/transactions/updateTransaction";
import { updateUnassignedFundAmount } from "@/funds/FundAssignmentEntryFrame";

/**
 * Props for the UpdateTransactionForm component.
 */
interface UpdateTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly transactionDebitAccount: Account | null;
  readonly transactionCreditAccount: Account | null;
  readonly transactionDebitFund: Fund | null;
  readonly transactionCreditFund: Fund | null;
  readonly funds: Fund[];
  readonly routeAccountingPeriod?: AccountingPeriod | null;
  readonly routeAccount?: Account | null;
  readonly routeFund?: Fund | null;
}

/**
 * Gets the URL to redirect the user to after successfully creating a transaction.
 */
const getRedirectUrl = function (
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): string {
  if (accountingPeriod !== null) {
    if (account !== null) {
      return accountingPeriodRoutes.accountDetail({
        id: accountingPeriod.id,
        accountId: account.id,
      });
    }
    if (fund !== null) {
      return accountingPeriodRoutes.fundDetail(
        {
          id: accountingPeriod.id,
          fundId: fund.id,
        },
        {},
      );
    }
    return accountingPeriodRoutes.detail(
      { id: accountingPeriod.id },
      { display: ToggleState.Transactions },
    );
  }
  if (account !== null) {
    return accountRoutes.detail({ id: account.id }, {});
  }
  if (fund !== null) {
    return fundRoutes.detail({ id: fund.id }, {});
  }
  return "";
};

/**
 * Component that displays the form for updating a transaction.
 */
const UpdateTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  transactionDebitAccount,
  transactionCreditAccount,
  transactionDebitFund,
  transactionCreditFund,
  funds,
  routeAccountingPeriod,
  routeAccount,
  routeFund,
}: UpdateTransactionFormProps): JSX.Element {
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;

  const [date, setDate] = useState<Dayjs | null>(null);
  const [location, setLocation] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [amount, setAmount] = useState<number | null>(null);

  const [incomeFundAssignments, setIncomeFundAssignments] = useState<
    FundAmount[]
  >([]);

  const [spendingFundAssignments, setSpendingFundAssignments] = useState<
    FundAmount[]
  >([]);

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

  let request: UpdateTransactionRequest | null = null;
  if (
    date !== null &&
    location !== "" &&
    description !== "" &&
    amount !== null &&
    amount > 0
  ) {
    if (
      transaction.type === TransactionModelIncomeTransactionModelType.Income &&
      isIncomeTransactionComplete(incomeFundAssignments)
    ) {
      request = {
        type: UpdateTransactionModelUpdateIncomeTransactionModelType.Income,
        date: date.format("yyyy-MM-DD"),
        location,
        description,
        amount,
        fundAssignments: incomeFundAssignments.map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
      };
    } else if (
      transaction.type ===
        TransactionModelSpendingTransactionModelType.Spending &&
      isSpendingTransactionComplete(spendingFundAssignments)
    ) {
      request = {
        type: UpdateTransactionModelUpdateSpendingTransactionModelType.Spending,
        date: date.format("yyyy-MM-DD"),
        location,
        description,
        amount,
        fundAssignments: spendingFundAssignments.map((fundAmount) => ({
          fundId: fundAmount.fundId,
          amount: fundAmount.amount,
        })),
      };
    } else if (
      transaction.type === TransactionModelAccountTransactionModelType.Account
    ) {
      request = {
        type: UpdateTransactionModelUpdateAccountTransactionModelType.Account,
        date: date.format("yyyy-MM-DD"),
        location,
        description,
        amount,
      };
    } else if (
      transaction.type === TransactionModelFundTransactionModelType.Fund
    ) {
      request = {
        type: UpdateTransactionModelUpdateFundTransactionModelType.Fund,
        date: date.format("yyyy-MM-DD"),
        location,
        description,
        amount,
      };
    }
  }

  const [state, action, pending] = useActionState(updateTransaction, {
    transactionId: transaction.id,
    redirectUrl: getRedirectUrl(
      routeAccountingPeriod ?? null,
      routeAccount ?? null,
      routeFund ?? null,
    ),
  });

  return (
    <Stack spacing={2}>
      <CreateOrUpdateTransactionDetailsFrame
        accountingPeriods={[]}
        accountingPeriod={transactionAccountingPeriod}
        setAccountingPeriod={null}
        date={date}
        setDate={setDate}
        location={location}
        setLocation={setLocation}
        description={description}
        setDescription={setDescription}
        amount={amount}
        setAmount={onAmountChange}
      />
      <CreateOrUpdateTransactionFromToFrame
        accounts={[]}
        debitAccount={transactionDebitAccount}
        creditAccount={transactionCreditAccount}
        funds={funds}
        debitFund={transactionDebitFund}
        creditFund={transactionCreditFund}
        setDebitFrom={null}
        setCreditTo={null}
      />
      {transaction.type ===
        TransactionModelIncomeTransactionModelType.Income && (
        <CreateOrUpdateIncomeTransactionFrame
          funds={funds}
          amount={amount}
          incomeFundAssignments={incomeFundAssignments}
          setIncomeFundAssignments={setIncomeFundAssignments}
        />
      )}
      {transaction.type ===
        TransactionModelSpendingTransactionModelType.Spending && (
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

export default UpdateTransactionForm;
