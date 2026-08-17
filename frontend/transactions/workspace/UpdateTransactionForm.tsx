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
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import type { Location } from "@/locations/types";
import { LocationProvider } from "@/locations/LocationProvider";
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
  readonly fundGoals: FundGoalWithProgress[];
  readonly locations: Location[];
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
  fundGoals,
  locations,
  redirectUrl,
}: UpdateTransactionFormProps): JSX.Element | null {
  let form: JSX.Element | null = null;
  if (isIncomeTransaction(transaction)) {
    form = (
      <UpdateIncomeTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        funds={funds}
        fundGoals={fundGoals}
        redirectUrl={redirectUrl}
      />
    );
  } else if (isSpendingTransaction(transaction)) {
    form = (
      <UpdateSpendingTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        funds={funds}
        fundGoals={fundGoals}
        redirectUrl={redirectUrl}
      />
    );
  } else if (isAccountTransaction(transaction)) {
    form = (
      <UpdateAccountTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={accounts}
        redirectUrl={redirectUrl}
      />
    );
  } else if (isFundTransaction(transaction)) {
    form = (
      <UpdateFundTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        funds={funds}
        redirectUrl={redirectUrl}
      />
    );
  }
  return <LocationProvider locations={locations}>{form}</LocationProvider>;
};

export default UpdateTransactionForm;
