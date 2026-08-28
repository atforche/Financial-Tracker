"use client";

import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/fund/helpers";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import FundTransactionForm from "@/transactions/workspace/fund/FundTransactionForm";
import type { FundWithBalance } from "@/funds/types";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { getDefaultDate } from "@/transactions/workspace/helpers";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateFundTransactionForm component.
 */
interface CreateFundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: FundWithBalance[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for fund transfer transactions.
 */
const CreateFundTransactionForm = function ({
  accountingPeriods,
  funds,
  redirectUrl,
  details,
}: CreateFundTransactionFormProps): JSX.Element {
  const {
    accountingPeriod,
    setAccountingPeriod,
    date,
    setDate,
    description,
    setDescription,
  } = details;
  const defaultDate = getDefaultDate(accountingPeriod);
  const [source, setSource] = useState<FundSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>([
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
    <FundTransactionForm<CreateTransactionRequest>
      formRef={formRef}
      funds={funds}
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

export default CreateFundTransactionForm;
