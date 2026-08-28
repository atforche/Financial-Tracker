"use client";

import { type JSX, useState } from "react";
import {
  type RefundDestinationDraft,
  type RefundSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/refund/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import RefundTransactionForm from "@/transactions/workspace/refund/RefundTransactionForm";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { getDefaultDate } from "@/transactions/workspace/helpers";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateRefundTransactionForm component.
 */
interface CreateRefundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for refund transactions.
 */
const CreateRefundTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
  details,
}: CreateRefundTransactionFormProps): JSX.Element {
  const {
    accountingPeriod,
    setAccountingPeriod,
    date,
    setDate,
    description,
    setDescription,
  } = details;
  const defaultDate = getDefaultDate(accountingPeriod);
  const [sources, setSources] = useState<RefundSourceDraft[]>([
    createEmptySource(),
  ]);
  const [destination, setDestination] = useState<RefundDestinationDraft>(
    createEmptyDestination(),
  );

  const { formRef, state, pending, reset, submit } = useCreateTransactionEditor(
    {
      redirectUrl,
      resetDraft: (): void => {
        details.reset();
        setSources([createEmptySource()]);
        setDestination(createEmptyDestination());
      },
    },
  );

  const request: CreateTransactionRequest | null = buildCreateRequest(
    accountingPeriod,
    date,
    defaultDate,
    description,
    sources,
    destination,
  );

  return (
    <RefundTransactionForm<CreateTransactionRequest>
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
      sources={sources}
      setSources={setSources}
      destination={destination}
      setDestination={setDestination}
      submitLabel="Create"
      state={state}
      pending={pending}
      request={request}
      onReset={reset}
      onSubmit={submit}
    />
  );
};

export default CreateRefundTransactionForm;
