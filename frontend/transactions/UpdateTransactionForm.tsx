"use client";

import { Button, DialogActions, Stack } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import { type JSX, startTransition, useActionState, useState } from "react";
import {
  type Transaction,
  TransactionType,
  UpdateAccountTransactionType,
  UpdateFundTransactionType,
  UpdateIncomeTransactionType,
  UpdateSpendingTransactionType,
  type UpdateTransactionRequest,
  isIncomeTransactionComplete,
  isSpendingTransactionComplete,
} from "@/transactions/types";
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
import breadcrumbs from "@/transactions/breadcrumbs";
import routes from "@/transactions/routes";
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
  transaction: Transaction,
  accountingPeriod: AccountingPeriod | null,
  account: Account | null,
  fund: Fund | null,
): string {
  return routes.detail(
    { id: transaction.id },
    {
      accountingPeriodId: accountingPeriod?.id ?? null,
      accountId: account?.id ?? null,
      fundId: fund?.id ?? null,
    },
  );
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

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [location, setLocation] = useState<string>(transaction.location);
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [amount, setAmount] = useState<number | null>(transaction.amount);

  const [incomeFundAssignments, setIncomeFundAssignments] = useState<
    FundAmount[]
  >(
    transaction.transactionType === TransactionType.Income &&
      "fundAssignments" in transaction
      ? updateUnassignedFundAmount(
          unassignedFund,
          transaction.amount,
          transaction.fundAssignments,
        )
      : [],
  );

  const [spendingFundAssignments, setSpendingFundAssignments] = useState<
    FundAmount[]
  >(
    transaction.transactionType === TransactionType.Spending &&
      "fundAssignments" in transaction
      ? updateUnassignedFundAmount(
          unassignedFund,
          transaction.amount,
          transaction.fundAssignments,
        )
      : [],
  );

  /**
   * Event handler for when the amount field is changed in the create transaction form.
   */
  const onAmountChange = function (newAmount: number | null): void {
    setAmount(newAmount);

    setIncomeFundAssignments(
      updateUnassignedFundAmount(
        unassignedFund,
        newAmount,
        incomeFundAssignments,
      ),
    );
    setSpendingFundAssignments(
      updateUnassignedFundAmount(
        unassignedFund,
        newAmount,
        spendingFundAssignments,
      ),
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
      transaction.transactionType === TransactionType.Income &&
      isIncomeTransactionComplete(incomeFundAssignments)
    ) {
      request = {
        type: UpdateIncomeTransactionType.Income,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
        fundAssignments: incomeFundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
      };
    } else if (
      transaction.transactionType === TransactionType.Spending &&
      isSpendingTransactionComplete(spendingFundAssignments)
    ) {
      request = {
        type: UpdateSpendingTransactionType.Spending,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
        fundAssignments: spendingFundAssignments
          .filter((fundAmount) => fundAmount.fundName !== "Unassigned")
          .map((fundAmount) => ({
            fundId: fundAmount.fundId,
            amount: fundAmount.amount,
          })),
      };
    } else if (transaction.transactionType === TransactionType.Account) {
      request = {
        type: UpdateAccountTransactionType.Account,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
      };
    } else if (transaction.transactionType === TransactionType.Fund) {
      request = {
        type: UpdateFundTransactionType.Fund,
        date: date.format("YYYY-MM-DD"),
        location,
        description,
        amount,
      };
    }
  }

  const [state, action, pending] = useActionState(updateTransaction, {
    transactionId: transaction.id,
    redirectUrl: getRedirectUrl(
      transaction,
      routeAccountingPeriod ?? null,
      routeAccount ?? null,
      routeFund ?? null,
    ),
  });

  return (
    <Stack spacing={2}>
      <Breadcrumbs
        breadcrumbs={breadcrumbs.update(
          transaction,
          routeAccountingPeriod ?? null,
          routeAccount ?? null,
          routeFund ?? null,
        )}
      />
      <Stack spacing={2} sx={{ maxWidth: "600px" }}>
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
        {transaction.transactionType === TransactionType.Income && (
          <CreateOrUpdateIncomeTransactionFrame
            funds={funds}
            amount={amount}
            incomeFundAssignments={incomeFundAssignments}
            setIncomeFundAssignments={setIncomeFundAssignments}
          />
        )}
        {transaction.transactionType === TransactionType.Spending && (
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
            Update
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

export default UpdateTransactionForm;
