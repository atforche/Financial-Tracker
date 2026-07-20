"use client";

import {
  type Transaction,
  isAccountTransaction,
  isFundTransaction,
  isIncomeTransaction,
  isSpendingTransaction,
} from "@/transactions/types";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundPlanWithProgress } from "@/goals/types";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import UpdateAccountTransactionForm from "@/transactions/workspace/account/UpdateAccountTransactionForm";
import UpdateFundTransactionForm from "@/transactions/workspace/fund/UpdateFundTransactionForm";
import UpdateIncomeTransactionForm from "@/transactions/workspace/income/UpdateIncomeTransactionForm";
import UpdateSpendingTransactionForm from "@/transactions/workspace/spending/UpdateSpendingTransactionForm";

/**
 * Props for the UpdateTransactionForm component.
 */
interface UpdateTransactionFormProps {
  readonly transaction: Transaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundPlans: FundPlanWithProgress[];
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
  fundPlans,
  redirectUrl,
}: UpdateTransactionFormProps): JSX.Element | null {
  if (isIncomeTransaction(transaction)) {
    return (
      <UpdateIncomeTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        funds={funds}
        fundPlans={fundPlans}
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
        fundPlans={fundPlans}
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
