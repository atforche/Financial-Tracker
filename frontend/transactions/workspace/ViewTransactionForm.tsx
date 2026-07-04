"use client";

/* eslint-disable sort-imports */

import type { AccountingPeriod } from "@/accounting-periods/types";
import { Button, Stack } from "@mui/material";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import { type Transaction, TransactionType } from "@/transactions/transaction";
import { asAccountTransaction } from "@/transactions/accountTransaction";
import { asFundTransaction } from "@/transactions/fundTransaction";
import { asIncomeTransaction } from "@/transactions/incomeTransaction";
import { asSpendingTransaction } from "@/transactions/spendingTransaction";
import { getPostedTransactionAccounts } from "@/transactions/postingHelpers";
import AccountTransactionDestinationViewFrame from "@/transactions/workspace/account/AccountTransactionDestinationViewFrame";
import AccountTransactionSourceViewFrame from "@/transactions/workspace/account/AccountTransactionSourceViewFrame";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import FundTransactionDestinationViewFrame from "@/transactions/workspace/fund/FundTransactionDestinationViewFrame";
import FundTransactionSourceViewFrame from "@/transactions/workspace/fund/FundTransactionSourceViewFrame";
import IncomeTransactionDestinationViewFrame from "@/transactions/workspace/income/IncomeTransactionDestinationViewFrame";
import IncomeTransactionSourceViewFrame from "@/transactions/workspace/income/IncomeTransactionSourceViewFrame";
import Link from "next/link";
import SpendingTransactionDestinationViewFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationViewFrame";
import SpendingTransactionSourceViewFrame from "@/transactions/workspace/spending/SpendingTransactionSourceViewFrame";
import TransactionDetailsViewSection from "@/transactions/workspace/TransactionDetailsViewSection";
import TransactionSourceDestinationLayout from "@/transactions/workspace/TransactionSourceDestinationLayout";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";

/**
 * Props for the ViewTransactionForm component.
 */
interface ViewTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: Fund[];
  readonly currentUrl: string;
  readonly workspaceUrl: string;
  readonly editUrl: string;
}

/**
 * Displays a read-only transaction detail view for the selected transaction.
 */
const ViewTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
  currentUrl,
  workspaceUrl,
  editUrl,
}: ViewTransactionFormProps): JSX.Element {
  const spendingTransaction = asSpendingTransaction(transaction);
  const incomeTransaction = asIncomeTransaction(transaction);
  const accountTransaction = asAccountTransaction(transaction);
  const fundTransaction = asFundTransaction(transaction);
  const postedAccountCount = getPostedTransactionAccounts(transaction).length;

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionDetailsViewSection
        accountingPeriod={transactionAccountingPeriod}
        date={transaction.date}
        description={transaction.description}
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
              redirectUrl={workspaceUrl}
            />
          </Stack>
        }
      />

      {transaction.transactionType === TransactionType.Spending &&
      spendingTransaction !== null ? (
        <TransactionSourceDestinationLayout
          sourceFrame={
            <SpendingTransactionSourceViewFrame
              transaction={spendingTransaction}
              account={spendingTransaction.source.account}
            />
          }
          destinationFrames={spendingTransaction.destinations.map(
            (destination, index) => (
              <SpendingTransactionDestinationViewFrame
                key={`spending-destination-${index}`}
                transaction={spendingTransaction}
                index={index}
                funds={funds}
                account={destination.account ?? null}
                location={destination.location ?? null}
                amount={destination.amount}
                fundAssignments={destination.fundAssignments}
              />
            ),
          )}
        />
      ) : null}

      {transaction.transactionType === TransactionType.Income &&
      incomeTransaction !== null ? (
        <TransactionSourceDestinationLayout
          sourceFrame={
            <IncomeTransactionSourceViewFrame
              transaction={incomeTransaction}
              account={incomeTransaction.source.account ?? null}
              location={incomeTransaction.source.location ?? null}
              incomeLines={incomeTransaction.source.incomeLines}
              incomeDeductions={incomeTransaction.source.incomeDeductions}
            />
          }
          destinationFrames={incomeTransaction.destinations.map(
            (destination, index) => (
              <IncomeTransactionDestinationViewFrame
                key={`income-destination-${index}`}
                transaction={incomeTransaction}
                index={index}
                funds={funds}
                account={destination.account}
                amount={destination.amount}
                fundAssignments={destination.fundAssignments}
              />
            ),
          )}
        />
      ) : null}

      {transaction.transactionType === TransactionType.Account &&
      accountTransaction !== null ? (
        <TransactionSourceDestinationLayout
          sourceFrame={
            <AccountTransactionSourceViewFrame
              transaction={accountTransaction}
              account={accountTransaction.source.account ?? null}
              location={accountTransaction.source.location ?? ""}
              amount={accountTransaction.amount}
            />
          }
          destinationFrames={accountTransaction.destinations.map(
            (destination, index) => (
              <AccountTransactionDestinationViewFrame
                key={`account-destination-${index}`}
                transaction={accountTransaction}
                index={index}
                account={destination.account ?? null}
                location={destination.location ?? ""}
                amount={destination.amount}
              />
            ),
          )}
        />
      ) : null}

      {transaction.transactionType === TransactionType.Fund &&
      fundTransaction !== null ? (
        <TransactionSourceDestinationLayout
          sourceFrame={
            <FundTransactionSourceViewFrame
              fund={fundTransaction.source.fund}
              amount={fundTransaction.amount}
            />
          }
          destinationFrames={fundTransaction.destinations.map(
            (destination, index) => (
              <FundTransactionDestinationViewFrame
                key={`fund-destination-${index}`}
                index={index}
                fund={destination.fund}
                amount={destination.fund.newFundBalance.postedBalance}
              />
            ),
          )}
        />
      ) : null}
    </Stack>
  );
};

export default ViewTransactionForm;
