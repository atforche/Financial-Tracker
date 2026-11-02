"use client";

import type {
  AccountingPeriod,
  ExpectedIncomeSourcesByPeriod,
} from "@/accounting-periods/types";
import {
  type IncomeDestinationDraft,
  type IncomeSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/income/helpers";
import { type JSX, useState } from "react";
import type { AccountWithBalance } from "@/accounts/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import IncomeTransactionForm from "@/transactions/workspace/income/IncomeTransactionForm";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateIncomeTransactionForm component.
 */
interface CreateIncomeTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly expectedIncomeSources: ExpectedIncomeSourcesByPeriod;
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for income transactions.
 */
const CreateIncomeTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  expectedIncomeSources,
  redirectUrl,
  details,
}: CreateIncomeTransactionFormProps): JSX.Element {
  const [source, setSource] = useState<IncomeSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>([
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
    <IncomeTransactionForm<CreateTransactionRequest>
      {...formProps}
      submitLabel="Create Income Transaction"
      accounts={accounts}
      funds={funds}
      fundGoals={fundGoals}
      expectedIncomeSources={expectedIncomeSources}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default CreateIncomeTransactionForm;
