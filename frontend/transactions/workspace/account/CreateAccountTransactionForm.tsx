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
import { getDefaultDate } from "@/transactions/workspace/helpers";
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
  const {
    accountingPeriod,
    setAccountingPeriod,
    date,
    setDate,
    description,
    setDescription,
  } = details;
  const defaultDate = getDefaultDate(accountingPeriod);
  const [source, setSource] = useState<AccountSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>([
    createEmptyDestination(),
  ]);

  const { formRef, state, pending, reset, submit } = useCreateTransactionEditor(
    {
      redirectUrl,
      resetDraft: (): void => {
        details.reset();
        setSource(createEmptySource());
        setDestinations([createEmptyDestination()]);
      },
    },
  );

  const request: CreateTransactionRequest | null = buildCreateRequest(
    accountingPeriod,
    date,
    defaultDate,
    description,
    source,
    destinations,
  );

  return (
    <AccountTransactionForm<CreateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      accountingPeriods={accountingPeriods}
      accountingPeriod={accountingPeriod}
      setAccountingPeriod={setAccountingPeriod}
      date={date}
      setDate={setDate}
      defaultDate={defaultDate}
      description={description}
      setDescription={setDescription}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
      submitLabel="Create"
      state={state}
      pending={pending}
      request={request}
      onReset={reset}
      onSubmit={submit}
    />
  );
};

export default CreateAccountTransactionForm;
