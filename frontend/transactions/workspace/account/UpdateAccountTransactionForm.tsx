"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/account/helpers";
import type {
  AccountTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import { type JSX, useState } from "react";
import AccountTransactionForm from "@/transactions/workspace/account/AccountTransactionForm";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateAccountTransactionForm component.
 */
interface UpdateAccountTransactionFormProps {
  readonly transaction: AccountTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for account transactions.
 */
const UpdateAccountTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  redirectUrl,
}: UpdateAccountTransactionFormProps): JSX.Element {
  const [source, setSource] = useState<AccountSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>(
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
    <AccountTransactionForm<UpdateTransactionRequest>
      {...formProps}
      accounts={accounts}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default UpdateAccountTransactionForm;
