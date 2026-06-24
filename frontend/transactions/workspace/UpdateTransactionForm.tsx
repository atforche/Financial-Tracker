"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { type Transaction, TransactionType } from "@/transactions/transaction";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import UpdateAccountTransactionForm from "@/transactions/workspace/UpdateAccountTransactionForm";
import UpdateFundTransactionForm from "@/transactions/workspace/UpdateFundTransactionForm";
import UpdateIncomeTransactionForm from "@/transactions/workspace/UpdateIncomeTransactionForm";
import UpdateSpendingTransactionForm from "@/transactions/workspace/UpdateSpendingTransactionForm";

interface UpdateTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: Account[];
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
  accounts,
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
        accounts={accounts}
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
        accounts={accounts}
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
        accounts={accounts}
        redirectUrl={redirectUrl}
      />
    );
  }

  return (
    <UpdateFundTransactionForm
      transaction={transaction}
      transactionAccountingPeriod={transactionAccountingPeriod}
      funds={funds}
      redirectUrl={redirectUrl}
    />
  );
};

export default UpdateTransactionForm;
