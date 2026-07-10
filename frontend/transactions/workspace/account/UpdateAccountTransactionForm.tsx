"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/account/helpers";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountTransaction } from "@/transactions/accountTransaction";
import AccountTransactionForm from "@/transactions/workspace/account/AccountTransactionForm";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { UpdateTransactionRequest } from "@/transactions/transaction";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateAccountTransactionForm component.
 */
interface UpdateAccountTransactionFormProps {
  readonly transaction: AccountTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: Account[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for account transactions.
 */
const UpdateAccountTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  redirectUrl,
}: UpdateAccountTransactionFormProps): JSX.Element {
  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<AccountSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>(
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
    description,
    source,
    destinations,
  );

  return (
    <AccountTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
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

export default UpdateAccountTransactionForm;
