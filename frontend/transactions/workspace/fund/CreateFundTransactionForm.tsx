"use client";

import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/fund/helpers";
import { type JSX, useState } from "react";
import {
  getDefaultAccountingPeriod,
  getDefaultDate,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { Dayjs } from "dayjs";
import FundTransactionForm from "@/transactions/workspace/fund/FundTransactionForm";
import type { FundWithBalance } from "@/funds/types";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateFundTransactionForm component.
 */
interface CreateFundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: FundWithBalance[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for fund transfer transactions.
 */
const CreateFundTransactionForm = function ({
  accountingPeriods,
  funds,
  redirectUrl,
}: CreateFundTransactionFormProps): JSX.Element {
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods),
    );
  const defaultDate = getDefaultDate(accountingPeriod);
  const [date, setDate] = useState<Dayjs | null>(null);
  const [description, setDescription] = useState<string>("");
  const [source, setSource] = useState<FundSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>([
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
