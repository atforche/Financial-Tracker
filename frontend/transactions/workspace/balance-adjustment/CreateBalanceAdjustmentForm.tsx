"use client";

import type {
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import { type JSX, useEffect, useRef, useState } from "react";
import {
  buildCreateRequest,
  getChange,
} from "@/transactions/workspace/balance-adjustment/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import BalanceAdjustmentForm from "@/transactions/workspace/balance-adjustment/BalanceAdjustmentForm";
import type { CreateTransactionRequest } from "@/transactions/types";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { getDefaultDate } from "@/transactions/workspace/helpers";
import { getLatestBalanceAdjustmentDescription } from "@/transactions/workspace/balance-adjustment/balanceAdjustmentLookups";
import { useCreateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";
import usePostedBalance from "@/transactions/workspace/balance-adjustment/usePostedBalance";

/**
 * Props for the CreateBalanceAdjustmentForm component.
 */
interface CreateBalanceAdjustmentFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly redirectUrl: string;
  readonly details: TransactionDetails;
}

/**
 * Displays the dedicated create form for balance adjustments.
 */
const CreateBalanceAdjustmentForm = function ({
  accountingPeriods,
  accounts,
  redirectUrl,
  details,
}: CreateBalanceAdjustmentFormProps): JSX.Element {
  const [account, setAccount] = useState<AccountBalanceEventDraft | null>(null);
  const [institutionBalance, setInstitutionBalance] = useState<number | null>(
    null,
  );
  // The description most recently filled in from an account, which a later account may replace
  const defaultedDescription = useRef<string | null>(null);
  // Identifies the latest description lookup, so an earlier account's slower answer is ignored
  const latestLookup = useRef(0);
  const accountId = account?.accountId ?? null;
  const date =
    (details.date ?? getDefaultDate(details.accountingPeriod))?.format(
      "YYYY-MM-DD",
    ) ?? null;
  const { balance: postedBalance, failed: postedBalanceFailed } =
    usePostedBalance(accountId, date);
  const change = getChange(institutionBalance, postedBalance);
  // A lookup that answers after this form is gone, such as after switching type, must not fill the shared description
  useEffect(
    () => (): void => {
      latestLookup.current += 1;
    },
    [],
  );
  const formProps = useCreateTransactionEditor({
    accountingPeriods,
    details,
    redirectUrl,
    resetDraft: (): void => {
      setAccount(null);
      setInstitutionBalance(null);
      defaultedDescription.current = null;
      latestLookup.current += 1;
    },
    buildRequest: (accountingPeriod, requestDate, defaultDate, description) =>
      buildCreateRequest(
        accountingPeriod,
        requestDate,
        defaultDate,
        description,
        accountId,
        change,
      ),
  });

  return (
    <BalanceAdjustmentForm<CreateTransactionRequest>
      {...formProps}
      accounts={accounts}
      account={account}
      setAccount={(nextAccount): void => {
        setAccount(nextAccount);
        latestLookup.current += 1;
        const lookup = latestLookup.current;
        const nextAccountId = nextAccount?.accountId ?? null;
        if (nextAccountId === null) {
          return;
        }
        // A description that is empty or was filled in from another account defaults to this account's last one
        getLatestBalanceAdjustmentDescription(nextAccountId)
          .then((latestDescription) => {
            if (lookup !== latestLookup.current) {
              return;
            }
            const previousDefault = defaultedDescription.current;
            defaultedDescription.current = latestDescription;
            details.setDescription((current) =>
              current.trim() === "" || current === previousDefault
                ? (latestDescription ?? "")
                : current,
            );
          })
          .catch(() => {
            // The description is left for the user to enter.
          });
      }}
      institutionBalance={institutionBalance}
      setInstitutionBalance={setInstitutionBalance}
      postedBalance={postedBalance}
      postedBalanceFailed={postedBalanceFailed}
      change={change}
    />
  );
};

export default CreateBalanceAdjustmentForm;
