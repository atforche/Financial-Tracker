"use client";

import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/income/helpers";
import { type JSX, useState } from "react";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import IncomeTransactionForm from "@/transactions/workspace/income/IncomeTransactionForm";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { getDefaultDate } from "@/transactions/workspace/helpers";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateIncomeTransactionForm component.
 */
interface CreateIncomeTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for income transactions.
 */
const CreateIncomeTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
  details,
}: CreateIncomeTransactionFormProps): JSX.Element {
  const {
    accountingPeriod,
    setAccountingPeriod,
    date,
    setDate,
    description,
    setDescription,
  } = details;
  const defaultDate = getDefaultDate(accountingPeriod);
  const [source, setSource] = useState<IncomeSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>([
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
    <IncomeTransactionForm<CreateTransactionRequest>
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
      submitLabel="Create Income Transaction"
      state={state}
      pending={pending}
      request={request}
      onReset={reset}
      onSubmit={submit}
    />
  );
};

export default CreateIncomeTransactionForm;
