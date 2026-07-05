"use client";

import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/spending/helpers";
import {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
} from "@/transactions/workspace/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateSpendingTransactionForm from "@/transactions/workspace/spending/CreateOrUpdateSpendingTransactionForm";
import type { CreateTransactionRequest } from "@/transactions/transaction";
import type { Dayjs } from "dayjs";
import type { Fund } from "@/funds/types";
import type { SpendingGoal } from "@/goals/types";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateSpendingTransactionForm component.
 */
interface CreateSpendingTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for spending transactions.
 */
const CreateSpendingTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  spendingGoals,
  redirectUrl,
}: CreateSpendingTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods),
    );
  const defaultDate = getDefaultDate(accountingPeriod);
  const [date, setDate] = useState<Dayjs | null>(defaultDate);
  const [description, setDescription] = useState<string>("");
  const [source, setSource] =
    useState<SpendingSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>([
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
    <CreateOrUpdateSpendingTransactionForm<CreateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      spendingGoals={spendingGoals}
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
      onSubmit={(nextRequest) => {
        action({ redirectUrl, request: nextRequest });
      }}
    />
  );
};

export default CreateSpendingTransactionForm;
