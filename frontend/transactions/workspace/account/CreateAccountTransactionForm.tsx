"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/account/helpers";
import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
} from "@/transactions/workspace/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateAccountTransactionForm from "@/transactions/workspace/account/CreateOrUpdateAccountTransactionForm";
import type { CreateTransactionRequest } from "@/transactions/transaction";
import type { Dayjs } from "dayjs";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateAccountTransactionForm component.
 */
interface CreateAccountTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
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
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

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

  const [state, action, pending] = useActionState(createTransaction, {});

  const reset = function (): void {
    setAccountingPeriod(getDefaultAccountingPeriod(accountingPeriods));
    setDate(null);
    setDescription("");
    setSource(createEmptySource());
    setDestinations([createEmptyDestination()]);
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true && state.transactionId !== null) {
      router.replace(
        redirectWithSelectedTransaction(redirectUrl, state.transactionId ?? ""),
        { scroll: false },
      );
    }
  }, [redirectUrl, router, state]);

  const request: CreateTransactionRequest | null = buildCreateRequest(
    accountingPeriod,
    date,
    defaultDate,
    description,
    source,
    destinations,
  );

  return (
    <CreateOrUpdateAccountTransactionForm<CreateTransactionRequest>
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
      transferFlowDescription="Build one source and one or more destinations. The destination amounts should add up to the transaction amount."
      submitLabel="Create"
      state={state}
      pending={pending}
      request={request}
      onReset={reset}
      onSubmit={(nextRequest) => {
        action({ redirectUrl, request: nextRequest });
      }}
    />
  );
};

export default CreateAccountTransactionForm;
