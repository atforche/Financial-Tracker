"use client";

import { type JSX, useState } from "react";
import {
  type RefundDestinationDraft,
  type RefundSourceDraft,
  buildUpdateRequest,
  getDestinationFromTransaction,
  getSourcesFromTransaction,
} from "@/transactions/workspace/refund/helpers";
import type {
  RefundTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import RefundTransactionForm from "@/transactions/workspace/refund/RefundTransactionForm";
import useTransactionPreview from "@/transactions/workspace/useTransactionPreview";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateRefundTransactionForm component.
 */
interface UpdateRefundTransactionFormProps {
  readonly transaction: RefundTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for refund transactions.
 */
const UpdateRefundTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
}: UpdateRefundTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [sources, setSources] = useState<RefundSourceDraft[]>(
    getSourcesFromTransaction(transaction),
  );
  const [destination, setDestination] = useState<RefundDestinationDraft>(
    getDestinationFromTransaction(transaction),
  );

  const { formRef, state, pending, reset, submit } = useUpdateTransactionEditor(
    {
      transactionId: transaction.id,
      redirectUrl,
      resetDraft: (): void => {
        setDate(dayjs(transaction.date));
        setDescription(transaction.description);
        setSources(getSourcesFromTransaction(transaction));
        setDestination(getDestinationFromTransaction(transaction));
      },
    },
  );

  const request: UpdateTransactionRequest | null = buildUpdateRequest(
    transactionAccountingPeriod,
    date,
    description,
    sources,
    destination,
  );
  const preview = useTransactionPreview(
    request === null ? null : { transactionId: transaction.id, request },
  );

  return (
    <RefundTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      fundGoals={fundGoals}
      accountingPeriods={[transactionAccountingPeriod]}
      accountingPeriod={transactionAccountingPeriod}
      setAccountingPeriod={null}
      date={date}
      setDate={setDate}
      defaultDate={null}
      description={description}
      setDescription={setDescription}
      sources={sources}
      setSources={setSources}
      destination={destination}
      setDestination={setDestination}
      submitLabel="Update"
      state={state}
      pending={pending}
      request={request}
      preview={preview}
      onReset={reset}
      onSubmit={submit}
    />
  );
};

export default UpdateRefundTransactionForm;
