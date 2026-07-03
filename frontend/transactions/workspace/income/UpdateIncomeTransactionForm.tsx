"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/income/helpers";
import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateIncomeTransactionForm from "@/transactions/workspace/income/CreateOrUpdateIncomeTransactionForm";
import type { Fund } from "@/funds/types";
import type { IncomeTransaction } from "@/transactions/incomeTransaction";
import type { UpdateTransactionRequest } from "@/transactions/transaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateIncomeTransactionForm component.
 */
interface UpdateIncomeTransactionFormProps {
  readonly transaction: IncomeTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
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
  spendingGoals,
  redirectUrl,
}: UpdateIncomeTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<IncomeSourceDraft>(
    getSourceFromTransaction(transaction, accounts),
  );
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>(
    getDestinationsFromTransaction(transaction, accounts),
  );

  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setDescription(transaction.description);
    setSource(getSourceFromTransaction(transaction, accounts));
    setDestinations(getDestinationsFromTransaction(transaction, accounts));
    focusFirstEntryControl(formRef.current);
  };

  useEffect(() => {
    if (state.success === true) {
      router.replace(redirectUrl, { scroll: false });
    }
  }, [redirectUrl, router, state.success]);

  const request: UpdateTransactionRequest | null = buildUpdateRequest(
    transactionAccountingPeriod,
    date,
    null,
    description,
    source,
    destinations,
  );

  return (
    <CreateOrUpdateIncomeTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      assignmentGoals={assignmentGoals}
      spendingGoals={spendingGoals}
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
      onSubmit={(nextRequest) => {
        action({
          transactionId: transaction.id,
          redirectUrl,
          request: nextRequest,
        });
      }}
    />
  );
};

export default UpdateIncomeTransactionForm;
