"use client";

import {
  type AccountDestinationDraft,
  type AccountSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/account/createOrUpdateAccountTransaction";
import {
  type JSX,
  useActionState,
  useEffect,
  useRef,
  useState,
} from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { Account } from "@/accounts/types";
import type { AccountTransaction } from "@/transactions/accountTransaction";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateAccountTransactionForm from "@/transactions/workspace/account/CreateOrUpdateAccountTransactionForm";
import type { UpdateTransactionRequest } from "@/transactions/transaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

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
  const formRef = useRef<HTMLDivElement | null>(null);
  const router = useRouter();

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<AccountSourceDraft>(
    getSourceFromTransaction(transaction, accounts),
  );
  const [destinations, setDestinations] = useState<AccountDestinationDraft[]>(
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
    description,
    source,
    destinations,
  );

  return (
    <CreateOrUpdateAccountTransactionForm<UpdateTransactionRequest>
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
      transferFlowDescription="Edit the source and each destination. The destination amounts should add up to the transaction amount."
      submitLabel="Update Account Transaction"
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

export default UpdateAccountTransactionForm;
