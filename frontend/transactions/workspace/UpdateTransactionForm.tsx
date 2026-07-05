"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import type { Transaction } from "@/transactions/transaction";
import UpdateAccountTransactionForm from "@/transactions/workspace/account/UpdateAccountTransactionForm";
import UpdateFundTransactionForm from "@/transactions/workspace/fund/UpdateFundTransactionForm";
import UpdateIncomeTransactionForm from "@/transactions/workspace/income/UpdateIncomeTransactionForm";
import UpdateSpendingTransactionForm from "@/transactions/workspace/spending/UpdateSpendingTransactionForm";
import { isAccountTransaction } from "@/transactions/accountTransaction";
import { isFundTransaction } from "@/transactions/fundTransaction";
import { isIncomeTransaction } from "@/transactions/incomeTransaction";
import { isSpendingTransaction } from "@/transactions/spendingTransaction";

/**
 * Props for the UpdateTransactionForm component.
 */
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
}: UpdateTransactionFormProps): JSX.Element | null {
  if (isIncomeTransaction(transaction)) {
    return (
      <UpdateIncomeTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        funds={funds}
        assignmentGoals={assignmentGoals}
        redirectUrl={redirectUrl}
      />
    );
  }
  if (isSpendingTransaction(transaction)) {
    return (
      <UpdateSpendingTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        funds={funds}
        spendingGoals={spendingGoals}
        redirectUrl={redirectUrl}
      />
    );
  }
  if (isAccountTransaction(transaction)) {
    return (
      <UpdateAccountTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        redirectUrl={redirectUrl}
      />
    );
  }
  if (isFundTransaction(transaction)) {
    return (
      <UpdateFundTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        funds={funds}
        redirectUrl={redirectUrl}
      />
    );
  }
  return null;
};

export default UpdateTransactionForm;
