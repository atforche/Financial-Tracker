"use client";

import { type JSX, useState } from "react";
import { Stack, Typography } from "@mui/material";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountTransactionForm from "@/transactions/workspace/account/CreateAccountTransactionForm";
import CreateFundTransactionForm from "@/transactions/workspace/fund/CreateFundTransactionForm";
import CreateIncomeTransactionForm from "@/transactions/workspace/income/CreateIncomeTransactionForm";
import CreateSpendingTransactionForm from "@/transactions/workspace/spending/CreateSpendingTransactionForm";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import PageLayout from "@/framework/view/PageLayout";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly redirectUrl: string;
  readonly showHeading?: boolean;
}

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  redirectUrl,
  showHeading = true,
}: CreateTransactionFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  type TransactionFormKind = "spending" | "income" | "account" | "fund";
  const [transactionType, setTransactionType] =
    useState<TransactionFormKind>("spending");

  if (!canWrite) {
    return null;
  }

  return (
    <PageLayout>
      {showHeading ? (
        <Stack spacing={0.5}>
          <Typography variant="h5">Create Transaction</Typography>
          <Typography variant="body2" color="text.secondary">
            Choose the transaction type, then complete the matching form.
          </Typography>
        </Stack>
      ) : null}
      <Stack spacing={1}>
        <ToggleButtonSelector
          value={transactionType}
          onChange={setTransactionType}
          options={[
            { value: "spending", label: "Spending" },
            { value: "income", label: "Income" },
            { value: "account", label: "Account" },
            { value: "fund", label: "Fund" },
          ]}
        />
      </Stack>

      {transactionType === "income" ? (
        <CreateIncomeTransactionForm
          accountingPeriods={accountingPeriods}
          accounts={accounts}
          funds={funds}
          fundGoals={fundGoals}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {transactionType === "spending" ? (
        <CreateSpendingTransactionForm
          accountingPeriods={accountingPeriods}
          accounts={accounts}
          funds={funds}
          fundGoals={fundGoals}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {transactionType === "account" ? (
        <CreateAccountTransactionForm
          accountingPeriods={accountingPeriods}
          accounts={accounts}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {transactionType === "fund" ? (
        <CreateFundTransactionForm
          accountingPeriods={accountingPeriods}
          funds={funds}
          redirectUrl={redirectUrl}
        />
      ) : null}
    </PageLayout>
  );
};

export default CreateTransactionForm;
