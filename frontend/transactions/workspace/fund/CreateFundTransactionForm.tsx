"use client";

import {
  type FundDestinationDraft,
  type FundSourceDraft,
  buildCreateRequest,
  createEmptyDestination,
  createEmptySource,
} from "@/transactions/workspace/fund/helpers";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { CreateTransactionRequest } from "@/transactions/types";
import FundTransactionForm from "@/transactions/workspace/fund/FundTransactionForm";
import type { FundWithBalance } from "@/funds/types";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the CreateFundTransactionForm component.
 */
interface CreateFundTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: FundWithBalance[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for fund transfer transactions.
 */
const CreateFundTransactionForm = function ({
  accountingPeriods,
  funds,
  redirectUrl,
  details,
}: CreateFundTransactionFormProps): JSX.Element {
  const [source, setSource] = useState<FundSourceDraft>(createEmptySource());
  const [destinations, setDestinations] = useState<FundDestinationDraft[]>([
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
    <FundTransactionForm<CreateTransactionRequest>
      {...formProps}
      funds={funds}
      source={source}
      setSource={setSource}
      destinations={destinations}
      setDestinations={setDestinations}
    />
  );
};

export default CreateFundTransactionForm;
