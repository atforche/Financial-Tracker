"use client";

import { type JSX, useState } from "react";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/spending/helpers";
import type {
  SpendingTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import SpendingTransactionForm from "@/transactions/workspace/spending/SpendingTransactionForm";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateSpendingTransactionForm component.
 */
interface UpdateSpendingTransactionFormProps {
  readonly transaction: SpendingTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for spending transactions.
 */
const UpdateSpendingTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
}: UpdateSpendingTransactionFormProps): JSX.Element {
  const [source, setSource] = useState<SpendingSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>(
    getDestinationsFromTransaction(transaction),
  );
  const formProps = useUpdateTransactionEditor({
    transaction,
    transactionAccountingPeriod,
    redirectUrl,
    resetDraft: (): void => {
      setSource(getSourceFromTransaction(transaction));
      setDestinations(getDestinationsFromTransaction(transaction));
    },
    buildRequest: (accountingPeriod, date, description) =>
      buildUpdateRequest(
        accountingPeriod,
        date,
        description,
        source,
        destinations,
      ),
  });

  return (
    <SpendingTransactionForm<UpdateTransactionRequest>
      {...formProps}
      accounts={accounts}
      funds={funds}
      fundGoals={fundGoals}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default UpdateSpendingTransactionForm;
