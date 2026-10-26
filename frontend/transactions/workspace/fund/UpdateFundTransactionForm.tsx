"use client";

import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildUpdateRequest,
  getDestinationsFromTransaction,
  getSourceFromTransaction,
} from "@/transactions/workspace/fund/helpers";
import type {
  FundTransaction,
  UpdateTransactionRequest,
} from "@/transactions/types";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import FundTransactionForm from "@/transactions/workspace/fund/FundTransactionForm";
import type { FundWithBalance } from "@/funds/types";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateFundTransactionForm component.
 */
interface UpdateFundTransactionFormProps {
  readonly transaction: FundTransaction;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly funds: FundWithBalance[];
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
  const [source, setSource] = useState<FundSourceDraft>(
    getSourceFromTransaction(transaction),
  );
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>(
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
        description,
        source,
        destinations,
      ),
  });

  return (
    <FundTransactionForm<UpdateTransactionRequest>
      {...formProps}
      funds={funds}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default UpdateFundTransactionForm;
