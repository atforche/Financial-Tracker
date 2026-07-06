"use client";

import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/fund/helpers";
import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { FundTransaction } from "@/transactions/fundTransaction";
import FundTransactionForm from "@/transactions/workspace/fund/FundTransactionForm";
import type { UpdateTransactionRequest } from "@/transactions/transaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import updateTransaction from "@/transactions/workspace/updateTransaction";
import { useRouter } from "next/navigation";

/**
 * Props for the UpdateFundTransactionForm component.
 */
interface UpdateFundTransactionFormProps {
  readonly transaction: FundTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: Fund[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for fund transfer transactions.
 */
const UpdateFundTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  funds,
  redirectUrl,
}: UpdateFundTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

  const [date, setDate] = useState<Dayjs | null>(dayjs(transaction.date));
  const [description, setDescription] = useState<string>(
    transaction.description,
  );
  const [source, setSource] = useState<FundSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>(
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
    <FundTransactionForm<UpdateTransactionRequest>
      formRef={formRef}
      funds={funds}
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

export default UpdateFundTransactionForm;
