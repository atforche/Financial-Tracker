"use client";

import {
  type AccountTransaction,
  type IncomeTransaction,
  type RefundTransaction,
  type SpendingTransaction,
  type Transaction,
  TransactionType,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asRefundTransaction,
  asSpendingTransaction,
} from "@/transactions/types";
import { Button, Stack } from "@mui/material";
import {
  PostingDateReporterContext,
  useTransactionPostingPreviews,
} from "@/transactions/workspace/useTransactionPostingPreviews";
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
  getDestinationFromTransaction as getRefundDestinationFromTransaction,
  getSourcesFromTransaction as getRefundSourcesFromTransaction,
} from "@/transactions/workspace/refund/helpers";
import {
  getDestinationsFromTransaction as getSpendingDestinationsFromTransaction,
  getSourceFromTransaction as getSpendingSourceFromTransaction,
} from "@/transactions/workspace/spending/helpers";
import AccountTransactionDestinationFrame from "@/transactions/workspace/account/AccountTransactionDestinationFrame";
import AccountTransactionSourceFrame from "@/transactions/workspace/account/AccountTransactionSourceFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import type { FundGoalWithProgress } from "@/fund-goals/types";
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
import { getCurrencyTotal } from "@/framework/currencyHelpers";
import { getPostedTransactionAccounts } from "@/transactions/postingHelpers";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ViewTransactionForm component.
 */
interface ViewTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly currentUrl: string;
  readonly editUrl: string;
  readonly deleteRedirectUrl: string;
}

const emptyFunds: FundWithBalance[] = [];

/**
 * Displays the read-only transaction detail view using the shared transaction form shell.
 */
const ViewTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
  fundGoals,
  currentUrl,
  editUrl,
  deleteRedirectUrl,
}: ViewTransactionFormProps): JSX.Element {
  const canWrite = useWriteAccess();
  const currentFundGoals = fundGoals.filter(
    (fundGoal) =>
      fundGoal.accountingPeriod?.id === transactionAccountingPeriod.id,
  );
  const spendingTransaction = asSpendingTransaction(transaction);
  const refundTransaction = asRefundTransaction(transaction);
  const incomeTransaction = asIncomeTransaction(transaction);
  const accountTransaction = asAccountTransaction(transaction);
  const fundTransaction = asFundTransaction(transaction);
  const postedAccountCount = getPostedTransactionAccounts(transaction).length;
  const { reportPostingDate, previews } =
    useTransactionPostingPreviews(transaction);

  /**
   * Gets the transaction as it would be after posting the provided account on its chosen posting date,
   * or the saved transaction when that posting is not being previewed.
   */
  const getPostingView = function (accountId: string | undefined): Transaction {
    return (
      (accountId === undefined ? undefined : previews[accountId]) ?? transaction
    );
  };

  let sourceContent: JSX.Element | JSX.Element[] | null = null;
  let destinationContent: JSX.Element[] = [];
  let sourceAmount: number | null = null;
  let destinationAmount = 0;

  if (
    transaction.transactionType === TransactionType.Spending &&
    spendingTransaction !== null
  ) {
    const source = getSpendingSourceFromTransaction(spendingTransaction);
    const sourceAccountId = spendingTransaction.source.account.account.id;
    const getSpendingView = (
      accountId: string | undefined,
    ): SpendingTransaction =>
      asSpendingTransaction(getPostingView(accountId)) ?? spendingTransaction;
    const getDestinationView = (index: number): SpendingTransaction =>
      getSpendingView(
        spendingTransaction.destinations[index]?.account?.account.id ??
          sourceAccountId,
      );
    const destinations =
      getSpendingDestinationsFromTransaction(spendingTransaction);

    sourceAmount = source.amount;
    destinationAmount = getCurrencyTotal(
      destinations.map((destination) => destination.amount),
    );
    sourceContent = (
      <SpendingTransactionSourceFrame
        readOnly
        accounts={[]}
        transaction={spendingTransaction}
        account={source.account}
        accountBalanceEvent={getSpendingView(sourceAccountId).source.account}
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
        fundGoals={currentFundGoals}
        transaction={spendingTransaction}
        account={destination.account}
        accountBalanceEvent={
          getDestinationView(index).destinations[index]?.account ?? null
        }
        fundBalanceEvents={
          getDestinationView(index).destinations[index]?.fundAssignments
        }
        setAccount={null}
        location={destination.location}
        setLocation={null}
        amount={destination.amount}
        setAmount={null}
        fundAssignments={destination.fundAssignments}
        setFundAssignments={null}
      />
    ));
  } else if (
    transaction.transactionType === TransactionType.Refund &&
    refundTransaction !== null
  ) {
    const sources = getRefundSourcesFromTransaction(refundTransaction);
    const destination = getRefundDestinationFromTransaction(refundTransaction);
    const destinationAccountId =
      refundTransaction.destination.account.account.id;
    const getRefundView = (accountId: string | undefined): RefundTransaction =>
      asRefundTransaction(getPostingView(accountId)) ?? refundTransaction;
    const getSourceView = (index: number): RefundTransaction =>
      getRefundView(
        refundTransaction.sources[index]?.account?.account.id ??
          destinationAccountId,
      );
    sourceAmount = getCurrencyTotal(sources.map((source) => source.amount));
    destinationAmount = sourceAmount;
    sourceContent = sources.map((source, index) => (
      <SpendingTransactionDestinationFrame
        key={`refund-source-${index}`}
        readOnly
        index={index}
        accounts={[]}
        funds={funds}
        fundGoals={currentFundGoals}
        transaction={refundTransaction}
        account={source.account}
        accountBalanceEvent={
          getSourceView(index).sources[index]?.account ?? null
        }
        fundBalanceEvents={getSourceView(index).sources[index]?.fundAssignments}
        setAccount={null}
        location={source.location}
        setLocation={null}
        amount={source.amount}
        setAmount={null}
        fundAssignments={source.fundAssignments}
        setFundAssignments={null}
        title="Source"
        accountCaption="Source Account"
        locationCaption="Source Location"
        entryCaption="Source"
      />
    ));
    destinationContent = [
      <SpendingTransactionSourceFrame
        key="refund-destination"
        readOnly
        accounts={[]}
        transaction={refundTransaction}
        account={destination.account}
        setAccount={null}
        amount={destinationAmount}
        setAmount={null}
        title="Destination"
        accountLabel="Destination Account"
        accountBalanceEvent={
          getRefundView(destinationAccountId).destination.account
        }
      />,
    ];
  } else if (
    transaction.transactionType === TransactionType.Income &&
    incomeTransaction !== null
  ) {
    const source = getIncomeSourceFromTransaction(incomeTransaction);
    const getIncomeView = (accountId: string | undefined): IncomeTransaction =>
      asIncomeTransaction(getPostingView(accountId)) ?? incomeTransaction;
    const getDestinationView = (index: number): IncomeTransaction =>
      getIncomeView(incomeTransaction.destinations[index]?.account.account.id);
    const destinations =
      getIncomeDestinationsFromTransaction(incomeTransaction);

    sourceAmount = getNetIncomeAmount(source);
    destinationAmount = getCurrencyTotal(
      destinations.map((destination) => destination.amount),
    );
    sourceContent = (
      <IncomeTransactionSourceFrame
        readOnly
        accounts={[]}
        transaction={incomeTransaction}
        account={source.account}
        accountBalanceEvent={
          getIncomeView(incomeTransaction.source.account?.account.id).source
            .account ?? null
        }
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
        fundGoals={currentFundGoals}
        transaction={incomeTransaction}
        account={destination.account}
        accountBalanceEvent={
          getDestinationView(index).destinations[index]?.account ?? null
        }
        fundBalanceEvents={
          getDestinationView(index).destinations[index]?.fundAssignments
        }
        fundGoalBalanceEvents={
          getDestinationView(index).destinations[index]?.fundGoals
        }
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
    const getAccountView = (
      accountId: string | undefined,
    ): AccountTransaction =>
      asAccountTransaction(getPostingView(accountId)) ?? accountTransaction;
    const destinations =
      getAccountDestinationsFromTransaction(accountTransaction);

    sourceAmount = source.amount;
    destinationAmount = getCurrencyTotal(
      destinations.map((destination) => destination.amount),
    );
    sourceContent = (
      <AccountTransactionSourceFrame
        readOnly
        accounts={[]}
        transaction={accountTransaction}
        account={source.account}
        accountBalanceEvent={
          getAccountView(accountTransaction.source.account?.account.id).source
            .account ?? null
        }
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
        accountBalanceEvent={
          getAccountView(
            accountTransaction.destinations[index]?.account?.account.id,
          ).destinations[index]?.account ?? null
        }
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
    destinationAmount = getCurrencyTotal(
      destinations.map((destination) => destination.amount),
    );
    sourceContent = (
      <FundTransactionSourceFrame
        readOnly
        funds={funds}
        fund={source.fund}
        fundBalanceEvent={fundTransaction.source.fund}
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
        fundBalanceEvent={fundTransaction.destinations[index]?.fund ?? null}
        setFund={null}
        amount={destination.amount}
        setAmount={null}
      />
    ));
  }

  return (
    <PostingDateReporterContext.Provider value={reportPostingDate}>
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
          !canWrite ? undefined : (
            <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
              <Button component={Link} href={editUrl} variant="contained">
                Edit
              </Button>
              {postedAccountCount > 0 ? (
                <UnpostTransactionForm
                  transaction={transaction}
                  redirectUrl={currentUrl}
                  label={postedAccountCount > 1 ? "Unpost All" : "Unpost"}
                />
              ) : (
                <DeleteTransactionForm
                  transaction={transaction}
                  redirectUrl={deleteRedirectUrl}
                />
              )}
            </Stack>
          )
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
        showSummary={transaction.transactionType === TransactionType.Income}
      />
    </PostingDateReporterContext.Provider>
  );
};

export default ViewTransactionForm;
