"use client";

import type {
  AccountingPeriod,
  ExpectedIncomeSourcesByPeriod,
} from "@/accounting-periods/types";
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
import type { AccountWithBalance } from "@/accounts/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
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
  readonly fundGoals: FundGoalWithProgress[];
  readonly expectedIncomeSources: ExpectedIncomeSourcesByPeriod;
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
  fundGoals,
  expectedIncomeSources,
  redirectUrl,
}: UpdateIncomeTransactionFormProps): JSX.Element {
  const [source, setSource] = useState<IncomeSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<IncomeDestinationDraft[]>(
    getDestinationsFromTransaction(transaction),
  );
  const formProps = useUpdateTransactionEditor({
    transaction,
    transactionAccountingPeriod,
    redirectUrl,
    resetDraft: (): void => {
      setSource(getSourceFromTransaction(transaction));
      setDestinations(getDestinationsFromTransaction(transaction));
    },
    buildRequest: (accountingPeriod, date, description) =>
      buildUpdateRequest(
        accountingPeriod,
        date,
        null,
        description,
        source,
        destinations,
      ),
  });

  return (
    <IncomeTransactionForm<UpdateTransactionRequest>
      {...formProps}
      submitLabel="Update Income Transaction"
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

export default UpdateIncomeTransactionForm;
