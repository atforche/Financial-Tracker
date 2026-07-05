"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { Button, Stack } from "@mui/material";
import { type Transaction, TransactionType } from "@/transactions/transaction";
import AccountTransactionDestinationViewFrame from "@/transactions/workspace/account/AccountTransactionDestinationViewFrame";
import AccountTransactionSourceViewFrame from "@/transactions/workspace/account/AccountTransactionSourceViewFrame";
import type { AccountingPeriod } from "@/accounting-periods/types";
import DeleteTransactionForm from "@/transactions/workspace/DeleteTransactionForm";
import type { Fund } from "@/funds/types";
import FundTransactionDestinationViewFrame from "@/transactions/workspace/fund/FundTransactionDestinationViewFrame";
import FundTransactionSourceViewFrame from "@/transactions/workspace/fund/FundTransactionSourceViewFrame";
import IncomeTransactionDestinationViewFrame from "@/transactions/workspace/income/IncomeTransactionDestinationViewFrame";
import IncomeTransactionSourceViewFrame from "@/transactions/workspace/income/IncomeTransactionSourceViewFrame";
import type { JSX } from "react";
import Link from "next/link";
import SpendingTransactionDestinationViewFrame from "@/transactions/workspace/spending/SpendingTransactionDestinationViewFrame";
import SpendingTransactionSourceViewFrame from "@/transactions/workspace/spending/SpendingTransactionSourceViewFrame";
import TransactionDetailsFrame from "@/transactions/workspace/TransactionDetailsFrame";
import TransactionSourceDestinationLayout from "@/transactions/workspace/TransactionSourceDestinationLayout";
import UnpostTransactionForm from "@/transactions/workspace/UnpostTransactionForm";
import { asAccountTransaction } from "@/transactions/accountTransaction";
import { asFundTransaction } from "@/transactions/fundTransaction";
import { asIncomeTransaction } from "@/transactions/incomeTransaction";
import { asSpendingTransaction } from "@/transactions/spendingTransaction";
import dayjs from "dayjs";
import { getPostedTransactionAccounts } from "@/transactions/postingHelpers";
import { getTransactionAccountDraftFromTransactionAccount } from "@/transactions/workspace/transactionAccountDraft";

/**
 * Props for the ViewTransactionForm component.
 */
interface ViewTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
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
  assignmentGoals,
  spendingGoals,
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
      <TransactionDetailsFrame
        accountingPeriods={[]}
        accountingPeriod={transactionAccountingPeriod}
        setAccountingPeriod={null}
        date={dayjs(transaction.date)}
        setDate={null}
        descriptionValue={transaction.description}
        setDescriptionValue={null}
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
              account={
                getTransactionAccountDraftFromTransactionAccount(
                  spendingTransaction.source.account,
                ) ?? null
              }
            />
          }
          destinationFrames={spendingTransaction.destinations.map(
            (destination, index) => (
              <SpendingTransactionDestinationViewFrame
                key={`spending-destination-${index}`}
                transaction={spendingTransaction}
                index={index}
                funds={funds}
                spendingGoals={spendingGoals}
                account={getTransactionAccountDraftFromTransactionAccount(
                  destination.account,
                )}
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
              account={getTransactionAccountDraftFromTransactionAccount(
                incomeTransaction.source.account,
              )}
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
                assignmentGoals={assignmentGoals}
                account={getTransactionAccountDraftFromTransactionAccount(
                  destination.account,
                )}
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
              account={getTransactionAccountDraftFromTransactionAccount(
                accountTransaction.source.account,
              )}
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
                account={getTransactionAccountDraftFromTransactionAccount(
                  destination.account,
                )}
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
