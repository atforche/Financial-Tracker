"use client";

import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/income/helpers";
import type {
  IncomeTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { AssignmentGoal } from "@/goals/types";
import type { FundWithBalance } from "@/funds/types";
import IncomeTransactionForm from "@/transactions/workspace/income/IncomeTransactionForm";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateIncomeTransactionForm component.
 */
interface UpdateIncomeTransactionFormProps {
  readonly transaction: IncomeTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for income transactions.
 */
const UpdateIncomeTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  assignmentGoals,
  redirectUrl,
}: UpdateIncomeTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<IncomeSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>(
    getDestinationsFromTransaction(transaction),
  );

  const { formRef, state, pending, reset, submit } = useUpdateTransactionEditor(
    {
      transactionId: transaction.id,
      redirectUrl,
      resetDraft: (): void => {
        setDate(dayjs(transaction.date));
        setDescription(transaction.description);
        setSource(getSourceFromTransaction(transaction));
        setDestinations(getDestinationsFromTransaction(transaction));
      },
    },
  );

  const request: UpdateTransactionRequest | null = buildUpdateRequest(
    transactionAccountingPeriod,
    date,
    null,
    description,
    source,
    destinations,
  );

  return (
    <IncomeTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      assignmentGoals={assignmentGoals}
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
      submitLabel="Update Income Transaction"
      state={state}
      pending={pending}
      request={request}
      onReset={reset}
      onSubmit={submit}
    />
  );
};

export default UpdateIncomeTransactionForm;
