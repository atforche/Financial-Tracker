"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { type Transaction, TransactionType } from "@/transactions/types";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import UpdateAccountTransactionForm from "@/transactions/workspace/UpdateAccountTransactionForm";
import UpdateFundTransactionForm from "@/transactions/workspace/UpdateFundTransactionForm";
import UpdateIncomeTransactionForm from "@/transactions/workspace/UpdateIncomeTransactionForm";
import UpdateSpendingTransactionForm from "@/transactions/workspace/UpdateSpendingTransactionForm";

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
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

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
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: UpdateTransactionFormProps): JSX.Element {
  if (transaction.transactionType === TransactionType.Income) {
    return (
      <UpdateIncomeTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        transactionDebitAccount={transactionDebitAccount}
        transactionCreditAccount={transactionCreditAccount}
        funds={funds}
        assignmentGoals={assignmentGoals}
        spendingGoals={spendingGoals}
        redirectUrl={redirectUrl}
      />
    );
  }

  if (transaction.transactionType === TransactionType.Spending) {
    return (
      <UpdateSpendingTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        transactionDebitAccount={transactionDebitAccount}
        transactionCreditAccount={transactionCreditAccount}
        funds={funds}
        assignmentGoals={assignmentGoals}
        spendingGoals={spendingGoals}
        redirectUrl={redirectUrl}
      />
    );
  }

  if (transaction.transactionType === TransactionType.Account) {
    return (
      <UpdateAccountTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        transactionDebitAccount={transactionDebitAccount}
        transactionCreditAccount={transactionCreditAccount}
        redirectUrl={redirectUrl}
      />
    );
  }

  return (
    <UpdateFundTransactionForm
      transaction={transaction}
      transactionAccountingPeriod={transactionAccountingPeriod}
      transactionDebitFund={transactionDebitFund}
      transactionCreditFund={transactionCreditFund}
      redirectUrl={redirectUrl}
    />
  );
};

export default UpdateTransactionForm;
