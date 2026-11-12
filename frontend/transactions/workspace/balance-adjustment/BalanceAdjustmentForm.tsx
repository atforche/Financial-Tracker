"use client";

import type {
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import BalanceAdjustmentFrame from "@/transactions/workspace/balance-adjustment/BalanceAdjustmentFrame";
import type { JSX } from "react";
import type { TransactionEditorFormProps } from "@/transactions/workspace/useTransactionEditor";
import TransactionForm from "@/transactions/workspace/TransactionForm";
import { asBalanceAdjustment } from "@/transactions/types";

/**
 * Props for the BalanceAdjustmentForm component.
 */
type BalanceAdjustmentFormProps<RequestPayload> =
  TransactionEditorFormProps<RequestPayload> & {
    readonly accounts: AccountWithBalance[];
    readonly account: AccountBalanceEventDraft | null;
    readonly setAccount: (account: AccountBalanceEventDraft | null) => void;
    readonly institutionBalance: number | null;
    readonly setInstitutionBalance: (balance: number | null) => void;
    readonly postedBalance: number | null;
    readonly postedBalanceFailed: boolean;
    readonly change: number | null;
  };

/**
 * Displays the shared balance adjustment form layout used by create and update flows.
 */
const BalanceAdjustmentForm = function <RequestPayload>({
  accounts,
  account,
  setAccount,
  institutionBalance,
  setInstitutionBalance,
  postedBalance,
  postedBalanceFailed,
  change,
  preview,
  ...formProps
}: BalanceAdjustmentFormProps<RequestPayload>): JSX.Element {
  const date = (formProps.date ?? formProps.defaultDate)?.format("YYYY-MM-DD");
  return (
    <TransactionForm<RequestPayload>
      {...formProps}
      content={
        <BalanceAdjustmentFrame
          accounts={accounts}
          account={account}
          setAccount={setAccount}
          balanceEvent={
            preview === null
              ? null
              : (asBalanceAdjustment(preview)?.account ?? null)
          }
          institutionBalance={institutionBalance}
          setInstitutionBalance={setInstitutionBalance}
          date={date ?? null}
          postedBalance={postedBalance}
          postedBalanceFailed={postedBalanceFailed}
          change={change}
          incomplete={account === null || change === null || change === 0}
        />
      }
    />
  );
};

export default BalanceAdjustmentForm;
