import { type Transaction, TransactionType } from "@/transactions/types";
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

/**
 * Displays a read-only transaction detail view for the selected transaction.
 */
const ViewTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
}: ViewTransactionFormProps): JSX.Element {
  const locationDetails =
    transaction.transactionType === TransactionType.Income &&
    "sourceLocation" in transaction &&
    transaction.sourceLocation !== null
      ? {
          label: "Source Location",
          value: transaction.sourceLocation,
        }
      : transaction.transactionType === TransactionType.Spending &&
          "destinationLocation" in transaction &&
          transaction.destinationLocation !== null
        ? {
            label: "Destination Location",
            value: transaction.destinationLocation,
          }
        : null;

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionDetailsViewSection
        accountingPeriod={transactionAccountingPeriod}
        date={transaction.date}
        locationLabel={locationDetails?.label ?? null}
        locationValue={locationDetails?.value ?? null}
        description={transaction.description}
        amount={transaction.amount}
      />

      {(transaction.transactionType === TransactionType.Income ||
        transaction.transactionType === TransactionType.Spending ||
        transaction.transactionType === TransactionType.Account) &&
      ("debitAccount" in transaction || "creditAccount" in transaction) ? (
        <TransactionAccountPathViewSection
          title={
            transaction.transactionType === TransactionType.Account
              ? "Transfer Path"
              : "Money Flow"
          }
          description={
            transaction.transactionType === TransactionType.Income
              ? "Review which tracked account receives the income and where the money originated."
              : transaction.transactionType === TransactionType.Spending
                ? "Review which tracked account is charged and where the money was paid."
                : "Review the source and destination accounts."
          }
          leftLabel={
            transaction.transactionType === TransactionType.Income
              ? "Source Account"
              : transaction.transactionType === TransactionType.Spending
                ? "Spend From"
                : "Debit From"
          }
          rightLabel={
            transaction.transactionType === TransactionType.Income
              ? "Deposit To"
              : transaction.transactionType === TransactionType.Spending
                ? "Pay To"
                : "Credit To"
          }
          leftAccount={transaction.debitAccount ?? null}
          rightAccount={transaction.creditAccount ?? null}
        />
      ) : null}

      {transaction.transactionType === TransactionType.Fund &&
      "debitFund" in transaction &&
      "creditFund" in transaction ? (
        <TransactionFundPathViewSection
          title="Transfer Path"
          description="Review the source fund and destination fund for this transfer."
          leftLabel="Debit From"
          rightLabel="Credit To"
          leftFund={transaction.debitFund}
          rightFund={transaction.creditFund}
        />
      ) : null}

      {(transaction.transactionType === TransactionType.Income ||
        transaction.transactionType === TransactionType.Spending) &&
      "fundAssignments" in transaction ? (
        <TransactionFundAssignmentsViewSection
          funds={funds}
          amount={transaction.amount}
          fundAssignments={transaction.fundAssignments}
          tone={
            transaction.transactionType === TransactionType.Income
              ? "income"
              : "spending"
          }
        />
      ) : null}
    </Stack>
  );
};

export default ViewTransactionForm;
