"use client";

import { type JSX, useState } from "react";
import {
  type SpendingDestinationDraft,
  type SpendingSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/spending/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import SpendingTransactionForm from "@/transactions/workspace/spending/SpendingTransactionForm";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateSpendingTransactionForm component.
 */
interface CreateSpendingTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for spending transactions.
 */
const CreateSpendingTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
  details,
}: CreateSpendingTransactionFormProps): JSX.Element {
  const [source, setSource] =
    useState<SpendingSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<SpendingDestinationDraft[]>([
    createEmptyDestination(),
  ]);
  const formProps = useCreateTransactionEditor({
    accountingPeriods,
    details,
    redirectUrl,
    resetDraft: (): void => {
      setSource(createEmptySource());
      setDestinations([createEmptyDestination()]);
    },
    buildRequest: (accountingPeriod, date, defaultDate, description) =>
      buildCreateRequest(
        accountingPeriod,
        date,
        defaultDate,
        description,
        source,
        destinations,
      ),
  });

  return (
    <SpendingTransactionForm<CreateTransactionRequest>
      {...formProps}
      accounts={accounts}
      funds={funds}
      fundGoals={fundGoals}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default CreateSpendingTransactionForm;
