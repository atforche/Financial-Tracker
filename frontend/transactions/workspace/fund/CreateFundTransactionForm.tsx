"use client";

import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/fund/helpers";
import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
} from "@/transactions/workspace/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateFundTransactionForm from "@/transactions/workspace/fund/CreateOrUpdateFundTransactionForm";
import type { CreateTransactionRequest } from "@/transactions/transaction";
import type { Dayjs } from "dayjs";
import type { Fund } from "@/funds/types";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateFundTransactionForm component.
 */
interface CreateFundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
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
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

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
    <CreateOrUpdateFundTransactionForm<CreateTransactionRequest>
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
      transferFlowDescription="Build one source and one or more destination funds. The destination amounts should add up to the transaction amount."
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

export default CreateFundTransactionForm;
