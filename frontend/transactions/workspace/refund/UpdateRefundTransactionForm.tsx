"use client";

import { type JSX, useState } from "react";
import {
  type RefundDestinationDraft,
  type RefundSourceDraft,
  buildUpdateRequest,
  getDestinationFromTransaction,
  getSourcesFromTransaction,
} from "@/transactions/workspace/refund/helpers";
import type {
  RefundTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import RefundTransactionForm from "@/transactions/workspace/refund/RefundTransactionForm";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateRefundTransactionForm component.
 */
interface UpdateRefundTransactionFormProps {
  readonly transaction: RefundTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for refund transactions.
 */
const UpdateRefundTransactionForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
}: UpdateRefundTransactionFormProps): JSX.Element {
  const [sources, setSources] = useState<RefundSourceDraft[]>(
    getSourcesFromTransaction(transaction),
  );
  const [destination, setDestination] = useState<RefundDestinationDraft>(
    getDestinationFromTransaction(transaction),
  );
  const formProps = useUpdateTransactionEditor({
    transaction,
    transactionAccountingPeriod,
    redirectUrl,
    resetDraft: (): void => {
      setSources(getSourcesFromTransaction(transaction));
      setDestination(getDestinationFromTransaction(transaction));
    },
    buildRequest: (accountingPeriod, date, description) =>
      buildUpdateRequest(
        accountingPeriod,
        date,
        description,
        sources,
        destination,
      ),
  });

  return (
    <RefundTransactionForm<UpdateTransactionRequest>
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

export default UpdateRefundTransactionForm;
