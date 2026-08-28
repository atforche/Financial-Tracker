"use client";

import { type JSX, useState } from "react";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/spending/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import SpendingTransactionForm from "@/transactions/workspace/spending/SpendingTransactionForm";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { getDefaultDate } from "@/transactions/workspace/helpers";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateSpendingTransactionForm component.
 */
interface CreateSpendingTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for spending transactions.
 */
const CreateSpendingTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
  details,
}: CreateSpendingTransactionFormProps): JSX.Element {
  const {
    accountingPeriod,
    setAccountingPeriod,
    date,
    setDate,
    description,
    setDescription,
  } = details;
  const defaultDate = getDefaultDate(accountingPeriod);
  const [source, setSource] =
    useState<SpendingSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>([
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
    <SpendingTransactionForm<CreateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      fundGoals={fundGoals}
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

export default CreateSpendingTransactionForm;
