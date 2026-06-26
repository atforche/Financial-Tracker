"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/income/helpers";
import { type JSX, useActionState, useEffect, useRef, useState } from "react";
import {
  getDefaultAccountingPeriod,
  getDefaultDate,
  redirectWithSelectedTransaction,
} from "@/transactions/workspace/helpers";
import type { Account } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateOrUpdateIncomeTransactionForm from "@/transactions/workspace/income/CreateOrUpdateIncomeTransactionForm";
import type { CreateTransactionRequest } from "@/transactions/transaction";
import type { Dayjs } from "dayjs";
import type { Fund } from "@/funds/types";
import createTransaction from "@/transactions/workspace/createTransaction";
import { focusFirstEntryControl } from "@/framework/forms/focusFirstEntryControl";
import { useRouter } from "next/navigation";

/**
 * Props for the CreateIncomeTransactionForm component.
 */
interface CreateIncomeTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: Account[];
  readonly funds: Fund[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated create form for income transactions.
 */
const CreateIncomeTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  assignmentGoals,
  spendingGoals,
  redirectUrl,
}: CreateIncomeTransactionFormProps): JSX.Element {
  const router = useRouter();
  const formRef = useRef<HTMLDivElement | null>(null);

  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods),
    );
  const defaultDate = getDefaultDate(accountingPeriod);
  const [date, setDate] = useState<Dayjs | null>(null);
  const [description, setDescription] = useState<string>("");
  const [source, setSource] = useState<IncomeSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>([
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
    <CreateOrUpdateIncomeTransactionForm<CreateTransactionRequest>
      formRef={formRef}
      accounts={accounts}
      funds={funds}
      assignmentGoals={assignmentGoals}
      spendingGoals={spendingGoals}
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
      incomeFlowDescription="Build one income source and one or more tracked destinations. The net source amount and destination amounts should both add up to the transaction amount."
      submitLabel="Create Income Transaction"
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

export default CreateIncomeTransactionForm;
