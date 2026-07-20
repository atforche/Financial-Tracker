"use client";

import { Button, Stack } from "@mui/material";
import {
  type Transaction,
  TransactionType,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
} from "@/transactions/types";
import {
  getDestinationsFromTransaction as getAccountDestinationsFromTransaction,
  getSourceFromTransaction as getAccountSourceFromTransaction,
} from "@/transactions/workspace/account/helpers";
import {
  getDestinationsFromTransaction as getFundDestinationsFromTransaction,
  getSourceFromTransaction as getFundSourceFromTransaction,
} from "@/transactions/workspace/fund/helpers";
import {
  getDestinationsFromTransaction as getIncomeDestinationsFromTransaction,
  getSourceFromTransaction as getIncomeSourceFromTransaction,
  getNetIncomeAmount,
} from "@/transactions/workspace/income/helpers";
import {
  getDestinationsFromTransaction as getSpendingDestinationsFromTransaction,
  getSourceFromTransaction as getSpendingSourceFromTransaction,
} from "@/transactions/workspace/spending/helpers";
import AccountTransactionDestinationFrame from "@/transactions/workspace/account/AccountTransactionDestinationFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/account/AccountTransactionSourceFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import type { FundPlanWithProgress } from "@/fund-plans/types";
import FundTransactionDestinationFrame from "@/transactions/workspace/fund/FundTransactionDestinationFrame";
import FundTransactionSourceFrame from "@/transactions/workspace/fund/FundTransactionSourceFrame";
import type { FundWithBalance } from "@/funds/types";
import IncomeTransactionDestinationFrame from "@/transactions/workspace/income/IncomeTransactionDestinationFrame";
import IncomeTransactionSourceFrame from "@/transactions/workspace/income/IncomeTransactionSourceFrame";
import type { JSX } from "react";
import Link from "next/link";
import SpendingTransactionDestinationFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationFrame";
import SpendingTransactionSourceFrame from "@/transactions/workspace/spending/SpendingTransactionSourceFrame";
import TransactionForm from "@/transactions/workspace/TransactionForm";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";
import dayjs from "dayjs";
import { getPostedTransactionAccounts } from "@/transactions/postingHelpers";

/**
 * Props for the ViewTransactionForm component.
 */
interface ViewTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: FundWithBalance[];
  readonly fundPlans: FundPlanWithProgress[];
  readonly currentUrl: string;
  readonly workspaceUrl: string;
  readonly editUrl: string;
  readonly returnUrl?: string | null;
}

const emptyFunds: FundWithBalance[] = [];

/**
 * Displays the read-only transaction detail view using the shared transaction form shell.
 */
const ViewTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
  fundPlans,
  currentUrl,
  workspaceUrl,
  editUrl,
  returnUrl = null,
}: ViewTransactionFormProps): JSX.Element {
  const spendingTransaction = asSpendingTransaction(transaction);
  const incomeTransaction = asIncomeTransaction(transaction);
  const accountTransaction = asAccountTransaction(transaction);
  const fundTransaction = asFundTransaction(transaction);
  const postedAccountCount = getPostedTransactionAccounts(transaction).length;

  let sourceContent: JSX.Element | null = null;
  let destinationContent: JSX.Element[] = [];
  let sourceAmount: number | null = null;
  let destinationAmount = 0;

  if (
    transaction.transactionType === TransactionType.Spending &&
    spendingTransaction !== null
  ) {
    const source = getSpendingSourceFromTransaction(spendingTransaction);
    const destinations =
      getSpendingDestinationsFromTransaction(spendingTransaction);

    sourceAmount = source.amount;
    destinationAmount = destinations.reduce(
      (total, destination) => total + (destination.amount ?? 0),
      0,
    );
    sourceContent = (
      <SpendingTransactionSourceFrame
        readOnly
        accounts={[]}
        transaction={spendingTransaction}
        account={source.account}
        setAccount={null}
        amount={source.amount}
        setAmount={null}
      />
    );
    destinationContent = destinations.map((destination, index) => (
      <SpendingTransactionDestinationFrame
        key={`spending-destination-${index}`}
        readOnly
        index={index}
        accounts={[]}
        funds={funds}
        fundPlans={fundPlans}
        transaction={spendingTransaction}
        account={destination.account}
        setAccount={null}
        location={destination.location}
        setLocation={null}
        amount={destination.amount}
        setAmount={null}
        fundAssignments={destination.fundAssignments}
        setFundAssignments={null}
        baselineFundAssignments={destination.baselineFundAssignments}
      />
    ));
  } else if (
    transaction.transactionType === TransactionType.Income &&
    incomeTransaction !== null
  ) {
    const source = getIncomeSourceFromTransaction(incomeTransaction);
    const destinations =
      getIncomeDestinationsFromTransaction(incomeTransaction);

    sourceAmount = getNetIncomeAmount(source);
    destinationAmount = destinations.reduce(
      (total, destination) => total + (destination.amount ?? 0),
      0,
    );
    sourceContent = (
      <IncomeTransactionSourceFrame
        readOnly
        accounts={[]}
        transaction={incomeTransaction}
        account={source.account}
        setAccount={null}
        location={source.location}
        setLocation={null}
        incomeLines={source.incomeLines}
        setIncomeLines={null}
        incomeDeductions={source.incomeDeductions}
        setIncomeDeductions={null}
      />
    );
    destinationContent = destinations.map((destination, index) => (
      <IncomeTransactionDestinationFrame
        key={`income-destination-${index}`}
        readOnly
        index={index}
        accounts={[]}
        funds={funds}
        fundPlans={fundPlans}
        transaction={incomeTransaction}
        account={destination.account}
        setAccount={null}
        amount={destination.amount}
        setAmount={null}
        fundAssignments={destination.fundAssignments}
        setFundAssignments={null}
        baselineFundAssignments={destination.baselineFundAssignments}
      />
    ));
  } else if (
    transaction.transactionType === TransactionType.Account &&
    accountTransaction !== null
  ) {
    const source = getAccountSourceFromTransaction(accountTransaction);
    const destinations =
      getAccountDestinationsFromTransaction(accountTransaction);

    sourceAmount = source.amount;
    destinationAmount = destinations.reduce(
      (total, destination) => total + (destination.amount ?? 0),
      0,
    );
    sourceContent = (
      <AccountTransactionSourceFrame
        readOnly
        accounts={[]}
        transaction={accountTransaction}
        account={source.account}
        setAccount={null}
        location={source.location}
        setLocation={null}
        amount={source.amount}
        setAmount={null}
      />
    );
    destinationContent = destinations.map((destination, index) => (
      <AccountTransactionDestinationFrame
        key={`account-destination-${index}`}
        readOnly
        index={index}
        accounts={[]}
        transaction={accountTransaction}
        account={destination.account}
        setAccount={null}
        location={destination.location}
        setLocation={null}
        amount={destination.amount}
        setAmount={null}
      />
    ));
  } else if (
    transaction.transactionType === TransactionType.Fund &&
    fundTransaction !== null
  ) {
    const source = getFundSourceFromTransaction(fundTransaction);
    const destinations = getFundDestinationsFromTransaction(fundTransaction);

    sourceAmount = source.amount;
    destinationAmount = destinations.reduce(
      (total, destination) => total + (destination.amount ?? 0),
      0,
    );
    sourceContent = (
      <FundTransactionSourceFrame
        readOnly
        funds={funds}
        fund={source.fund}
        setFund={null}
        amount={source.amount}
        setAmount={null}
      />
    );
    destinationContent = destinations.map((destination, index) => (
      <FundTransactionDestinationFrame
        key={`fund-destination-${index}`}
        readOnly
        index={index}
        funds={funds}
        fund={destination.fund}
        setFund={null}
        amount={destination.amount}
        setAmount={null}
      />
    ));
  }

  return (
    <TransactionForm
      readOnly
      accountingPeriods={[]}
      accountingPeriod={transactionAccountingPeriod}
      setAccountingPeriod={null}
      date={dayjs(transaction.date)}
      setDate={null}
      defaultDate={null}
      description={transaction.description}
      setDescription={null}
      headerContent={
        <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
          <Button component={Link} href={editUrl} variant="contained">
            Edit
          </Button>
          {postedAccountCount > 0 ? (
            <UnpostTransactionForm
              transaction={transaction}
              redirectUrl={currentUrl}
            />
          ) : null}
          <DeleteTransactionForm
            transaction={transaction}
            redirectUrl={returnUrl ?? workspaceUrl}
          />
        </Stack>
      }
      sourceContent={
        sourceContent ?? (
          <FundTransactionSourceFrame
            readOnly
            funds={emptyFunds}
            fund={null}
            setFund={null}
            amount={null}
            setAmount={null}
          />
        )
      }
      destinationContent={destinationContent}
      sourceAmount={sourceAmount}
      destinationAmount={destinationAmount}
      destinationCount={destinationContent.length}
    />
  );
};

export default ViewTransactionForm;
