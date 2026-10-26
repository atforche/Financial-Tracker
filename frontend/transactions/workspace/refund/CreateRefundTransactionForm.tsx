"use client";

import { type JSX, useState } from "react";
import {
  type RefundDestinationDraft,
  type RefundSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/refund/helpers";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import RefundTransactionForm from "@/transactions/workspace/refund/RefundTransactionForm";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateRefundTransactionForm component.
 */
interface CreateRefundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for refund transactions.
 */
const CreateRefundTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
  details,
}: CreateRefundTransactionFormProps): JSX.Element {
  const [sources, setSources] = useState<RefundSourceDraft[]>([
    createEmptySource(),
  ]);
  const [destination, setDestination] = useState<RefundDestinationDraft>(
    createEmptyDestination(),
  );
  const formProps = useCreateTransactionEditor({
    accountingPeriods,
    details,
    redirectUrl,
    resetDraft: (): void => {
      setSources([createEmptySource()]);
      setDestination(createEmptyDestination());
    },
    buildRequest: (accountingPeriod, date, defaultDate, description) =>
      buildCreateRequest(
        accountingPeriod,
        date,
        defaultDate,
        description,
        sources,
        destination,
      ),
  });

  return (
    <RefundTransactionForm<CreateTransactionRequest>
      {...formProps}
      accounts={accounts}
      funds={funds}
      fundGoals={fundGoals}
      sources={sources}
      setSources={setSources}
      destination={destination}
      setDestination={setDestination}
    />
  );
};

export default CreateRefundTransactionForm;
