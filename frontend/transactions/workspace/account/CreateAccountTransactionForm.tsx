"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/account/helpers";
import { type JSX, useState } from "react";
import AccountTransactionForm from "@/transactions/workspace/account/AccountTransactionForm";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateAccountTransactionForm component.
 */
interface CreateAccountTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for account transactions.
 */
const CreateAccountTransactionForm = function ({
  accountingPeriods,
  accounts,
  redirectUrl,
  details,
}: CreateAccountTransactionFormProps): JSX.Element {
  const [source, setSource] = useState<AccountSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>([
    createEmptyDestination(),
  ]);
  const formProps = useCreateTransactionEditor({
    accountingPeriods,
    details,
    redirectUrl,
    resetDraft: (): void => {
      setSource(createEmptySource());
      setDestinations([createEmptyDestination()]);
    },
    buildRequest: (accountingPeriod, date, defaultDate, description) =>
      buildCreateRequest(
        accountingPeriod,
        date,
        defaultDate,
        description,
        source,
        destinations,
      ),
  });

  return (
    <AccountTransactionForm<CreateTransactionRequest>
      {...formProps}
      accounts={accounts}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default CreateAccountTransactionForm;
