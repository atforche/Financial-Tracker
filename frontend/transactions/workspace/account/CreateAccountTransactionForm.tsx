"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/account/helpers";
import { type JSX, useState } from "react";
import {
  getDefaultAccountingPeriod,
  getDefaultDate,
} from "@/transactions/workspace/helpers";
import AccountTransactionForm from "@/transactions/workspace/account/AccountTransactionForm";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { Dayjs } from "dayjs";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateAccountTransactionForm component.
 */
interface CreateAccountTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for account transactions.
 */
const CreateAccountTransactionForm = function ({
  accountingPeriods,
  accounts,
  redirectUrl,
}: CreateAccountTransactionFormProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods),
    );
  const defaultDate = getDefaultDate(accountingPeriod);
  const [date, setDate] = useState<Dayjs | null>(null);
  const [description, setDescription] = useState<string>("");
  const [source, setSource] = useState<AccountSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>([
    createEmptyDestination(),
  ]);

  const { formRef, state, pending, reset, submit } = useCreateTransactionEditor(
    {
      redirectUrl,
      resetDraft: (): void => {
        setAccountingPeriod(getDefaultAccountingPeriod(accountingPeriods));
        setDate(null);
        setDescription("");
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
