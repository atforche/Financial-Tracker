"use client";

import { type JSX, useState } from "react";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/spending/helpers";
import type {
  SpendingTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundPlanWithProgress } from "@/fund-plans/types";
import type { FundWithBalance } from "@/funds/types";
import SpendingTransactionForm from "@/transactions/workspace/spending/SpendingTransactionForm";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateSpendingTransactionForm component.
 */
interface UpdateSpendingTransactionFormProps {
  readonly transaction: SpendingTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundPlans: FundPlanWithProgress[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for spending transactions.
 */
const UpdateSpendingTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  fundPlans,
  redirectUrl,
}: UpdateSpendingTransactionFormProps): JSX.Element {
  const currentFundPlans = fundPlans.filter(
    (fundPlan) =>
      fundPlan.accountingPeriod?.id === transactionAccountingPeriod.id,
  );
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<SpendingSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>(
    getDestinationsFromTransaction(transaction, currentFundPlans),
  );

  const { formRef, state, pending, reset, submit } = useUpdateTransactionEditor(
    {
      transactionId: transaction.id,
      redirectUrl,
      resetDraft: (): void => {
        setDate(dayjs(transaction.date));
        setDescription(transaction.description);
        setSource(getSourceFromTransaction(transaction));
        setDestinations(
          getDestinationsFromTransaction(transaction, currentFundPlans),
        );
      },
    },
  );

  const request: UpdateTransactionRequest | null = buildUpdateRequest(
    transactionAccountingPeriod,
    date,
    description,
    source,
    destinations,
  );

  return (
    <SpendingTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      fundPlans={fundPlans}
      accountingPeriods={[transactionAccountingPeriod]}
      accountingPeriod={transactionAccountingPeriod}
      setAccountingPeriod={null}
      date={date}
      setDate={setDate}
      defaultDate={null}
      description={description}
      setDescription={setDescription}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
      submitLabel="Update"
      state={state}
      pending={pending}
      request={request}
      onReset={reset}
      onSubmit={submit}
    />
  );
};

export default UpdateSpendingTransactionForm;
