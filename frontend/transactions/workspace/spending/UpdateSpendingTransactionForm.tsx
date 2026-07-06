"use client";

import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/spending/helpers";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { SpendingGoal } from "@/goals/types";
import type { SpendingTransaction } from "@/transactions/spendingTransaction";
import SpendingTransactionForm from "@/transactions/workspace/spending/SpendingTransactionForm";
import type { UpdateTransactionRequest } from "@/transactions/transaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateSpendingTransactionForm component.
 */
interface UpdateSpendingTransactionFormProps {
  readonly transaction: SpendingTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly spendingGoals: SpendingGoal[];
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
  spendingGoals,
  redirectUrl,
}: UpdateSpendingTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<SpendingSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>(
    getDestinationsFromTransaction(transaction),
  );

  const [state, action, pending] = useActionState(updateTransaction, {});

  const reset = function (): void {
    setDate(dayjs(transaction.date));
    setDescription(transaction.description);
    setSource(getSourceFromTransaction(transaction));
    setDestinations(getDestinationsFromTransaction(transaction));
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
    description,
    source,
    destinations,
  );

  return (
    <SpendingTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
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
      submitLabel="Update"
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

export default UpdateSpendingTransactionForm;
