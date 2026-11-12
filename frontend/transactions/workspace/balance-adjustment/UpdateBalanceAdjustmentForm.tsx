"use client";

import type {
  AccountBalanceEventDraft,
  AccountWithBalance,
} from "@/accounts/types";
import type {
  BalanceAdjustment,
  UpdateTransactionRequest,
} from "@/transactions/types";
import { type JSX, useState } from "react";
import {
  buildUpdateRequest,
  getBalanceWithoutAdjustment,
  getChange,
} from "@/transactions/workspace/balance-adjustment/helpers";
import dayjs, { type Dayjs } from "dayjs";
import type { AccountingPeriod } from "@/accounting-periods/types";
import BalanceAdjustmentForm from "@/transactions/workspace/balance-adjustment/BalanceAdjustmentForm";
import { getCurrencyTotal } from "@/framework/currencyHelpers";
import { getTransactionAccountDraftFromTransactionAccount } from "@/transactions/workspace/accountBalanceEventDraft";
import usePostedBalance from "@/transactions/workspace/balance-adjustment/usePostedBalance";
import { useUpdateTransactionEditor } from "@/transactions/workspace/useTransactionEditor";

/**
 * Props for the UpdateBalanceAdjustmentForm component.
 */
interface UpdateBalanceAdjustmentFormProps {
  readonly transaction: BalanceAdjustment;
  readonly transactionAccountingPeriod: AccountingPeriod;
  readonly accounts: AccountWithBalance[];
  readonly redirectUrl: string;
}

/**
 * Displays the dedicated update form for balance adjustments. Like creating one, it asks for the balance at the
 * institution and works out the change.
 */
const UpdateBalanceAdjustmentForm = function ({
  transaction,
  transactionAccountingPeriod,
  accounts,
  redirectUrl,
}: UpdateBalanceAdjustmentFormProps): JSX.Element {
  const savedAccount = getTransactionAccountDraftFromTransactionAccount(
    transaction.account,
  );
  // The posted date follows the date being edited, so the saved one is not shown
  const initialAccount =
    savedAccount === null ? null : { ...savedAccount, postedDate: null };
  const [account, setAccount] = useState<AccountBalanceEventDraft | null>(
    initialAccount,
  );
  // Undefined until the user enters a balance
  const [enteredBalance, setEnteredBalance] = useState<number | null>();
  const [transactionDate, setTransactionDate] = useState<Dayjs | null>(
    dayjs(transaction.date),
  );
  const accountId = account?.accountId ?? null;
  const date = transactionDate?.format("YYYY-MM-DD") ?? null;
  const { balance: lookedUpBalance, failed: postedBalanceFailed } =
    usePostedBalance(accountId, date);
  const postedBalance = getBalanceWithoutAdjustment(
    lookedUpBalance,
    accountId,
    date,
    transaction,
  );
  // Until a balance is entered, the saved change is kept, but only for the saved account
  let institutionBalance: number | null = null;
  if (enteredBalance !== undefined) {
    institutionBalance = enteredBalance;
  } else if (
    postedBalance !== null &&
    accountId === transaction.account.account.id
  ) {
    institutionBalance = getCurrencyTotal([postedBalance, transaction.amount]);
  }
  const change = getChange(institutionBalance, postedBalance);
  const formProps = useUpdateTransactionEditor({
    transaction,
    transactionAccountingPeriod,
    redirectUrl,
    dateState: { date: transactionDate, setDate: setTransactionDate },
    resetDraft: (): void => {
      setAccount(initialAccount);
      setEnteredBalance(undefined);
    },
    buildRequest: (accountingPeriod, requestDate, description) =>
      buildUpdateRequest(
        accountingPeriod,
        requestDate,
        description,
        accountId,
        change,
      ),
  });

  return (
    <BalanceAdjustmentForm<UpdateTransactionRequest>
      {...formProps}
      accounts={accounts}
      account={account}
      setAccount={setAccount}
      institutionBalance={institutionBalance}
      setInstitutionBalance={setEnteredBalance}
      postedBalance={postedBalance}
      postedBalanceFailed={postedBalanceFailed}
      change={change}
    />
  );
};

export default UpdateBalanceAdjustmentForm;
