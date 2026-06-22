import {
  type AccountTransaction,
  type FundTransaction,
  type IncomeTransaction,
  type SpendingTransaction,
  type Transaction,
  TransactionType,
  asAccountTransaction,
  asFundTransaction,
  asIncomeTransaction,
  asSpendingTransaction,
} from "@/transactions/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionAccountPathViewSection from "@/transactions/workspace/TransactionAccountPathViewSection";
import TransactionDetailsViewSection from "@/transactions/workspace/TransactionDetailsViewSection";
import TransactionFundAssignmentsViewSection from "@/transactions/workspace/TransactionFundAssignmentsViewSection";
import TransactionFundPathViewSection from "@/transactions/workspace/TransactionFundPathViewSection";

interface ViewTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: Fund[];
}

const renderIncomeView = function (
  transaction: IncomeTransaction,
  funds: Fund[],
): JSX.Element[] {
  return transaction.destinations.flatMap((destination, index) => {
    const title =
      transaction.destinations.length === 1
        ? "Money Flow"
        : `Money Flow ${index + 1}`;
    const destinationSection = (
      <TransactionAccountPathViewSection
        key={`income-path-${index}`}
        title={title}
        description="Review which tracked account receives this portion of the income and where the money originated."
        leftLabel="Source Account"
        rightLabel="Deposit To"
        leftAccount={transaction.source.account ?? null}
        rightAccount={destination.account}
        leftLocationLabel={transaction.source.account === null ? "Source Location" : null}
        leftLocationValue={transaction.source.location ?? null}
      />
    );
    const fundSection = (
      <TransactionFundAssignmentsViewSection
        key={`income-funds-${index}`}
        funds={funds}
        amount={destination.amount}
        fundAssignments={destination.fundAssignments}
        tone="income"
      />
    );
    return [destinationSection, fundSection];
  });
};

const renderSpendingView = function (
  transaction: SpendingTransaction,
  funds: Fund[],
): JSX.Element[] {
  return transaction.destinations.flatMap((destination, index) => {
    const title =
      transaction.destinations.length === 1
        ? "Money Flow"
        : `Money Flow ${index + 1}`;
    const destinationSection = (
      <TransactionAccountPathViewSection
        key={`spending-path-${index}`}
        title={title}
        description="Review which tracked account is charged and where this portion of the money was paid."
        leftLabel="Spend From"
        rightLabel="Pay To"
        leftAccount={transaction.source.account}
        rightAccount={destination.account ?? null}
        rightLocationLabel={destination.account === null ? "Destination Location" : null}
        rightLocationValue={destination.location ?? null}
      />
    );
    const fundSection = (
      <TransactionFundAssignmentsViewSection
        key={`spending-funds-${index}`}
        funds={funds}
        amount={destination.amount}
        fundAssignments={destination.fundAssignments}
        tone="spending"
      />
    );
    return [destinationSection, fundSection];
  });
};

const renderAccountView = function (
  transaction: AccountTransaction,
): JSX.Element[] {
  return transaction.destinations.map((destination, index) => (
    <TransactionAccountPathViewSection
      key={`account-path-${index}`}
      title={
        transaction.destinations.length === 1
          ? "Transfer Path"
          : `Transfer Path ${index + 1}`
      }
      description="Review the source and destination for this account transfer."
      leftLabel="Source"
      rightLabel="Destination"
      leftAccount={transaction.source.account ?? null}
      rightAccount={destination.account ?? null}
      leftLocationLabel={transaction.source.account === null ? "Source Location" : null}
      leftLocationValue={transaction.source.location ?? null}
      rightLocationLabel={destination.account === null ? "Destination Location" : null}
      rightLocationValue={destination.location ?? null}
    />
  ));
};

const renderFundView = function (transaction: FundTransaction): JSX.Element[] {
  return transaction.destinations.map((destination, index) => (
    <TransactionFundPathViewSection
      key={`fund-path-${index}`}
      title={
        transaction.destinations.length === 1
          ? "Transfer Path"
          : `Transfer Path ${index + 1}`
      }
      description="Review the source fund and destination fund for this transfer."
      leftLabel="Source Fund"
      rightLabel="Destination Fund"
      leftFund={transaction.source.fund}
      rightFund={destination.fund}
    />
  ));
};

/**
 * Displays a read-only transaction detail view for the selected transaction.
 */
const ViewTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
}: ViewTransactionFormProps): JSX.Element {
  const spendingTransaction = asSpendingTransaction(transaction);
  const incomeTransaction = asIncomeTransaction(transaction);
  const accountTransaction = asAccountTransaction(transaction);
  const fundTransaction = asFundTransaction(transaction);

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionDetailsViewSection
        accountingPeriod={transactionAccountingPeriod}
        date={transaction.date}
        description={transaction.description}
        amount={transaction.amount}
      />

      {transaction.transactionType === TransactionType.Spending &&
      spendingTransaction !== null
        ? renderSpendingView(spendingTransaction, funds)
        : null}

      {transaction.transactionType === TransactionType.Income &&
      incomeTransaction !== null
        ? renderIncomeView(incomeTransaction, funds)
        : null}

      {transaction.transactionType === TransactionType.Account &&
      accountTransaction !== null
        ? renderAccountView(accountTransaction)
        : null}

      {transaction.transactionType === TransactionType.Fund &&
      fundTransaction !== null
        ? renderFundView(fundTransaction)
        : null}
    </Stack>
  );
};

export default ViewTransactionForm;
