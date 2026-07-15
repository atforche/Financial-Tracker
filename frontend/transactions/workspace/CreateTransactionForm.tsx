"use client";

import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import { type JSX, useState } from "react";
import {
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountTransactionForm from "@/transactions/workspace/account/CreateAccountTransactionForm";
import CreateFundTransactionForm from "@/transactions/workspace/fund/CreateFundTransactionForm";
import CreateIncomeTransactionForm from "@/transactions/workspace/income/CreateIncomeTransactionForm";
import CreateSpendingTransactionForm from "@/transactions/workspace/spending/CreateSpendingTransactionForm";
import type { FundWithBalance } from "@/funds/types";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly spendingGoals: SpendingGoal[];
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
  assignmentGoals,
  spendingGoals,
  redirectUrl,
  showHeading = true,
}: CreateTransactionFormProps): JSX.Element {
  type TransactionFormKind = "spending" | "income" | "account" | "fund";
  const [transactionType, setTransactionType] =
    useState<TransactionFormKind>("spending");

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      {showHeading ? (
        <Stack spacing={0.5}>
          <Typography variant="h5">Create Transaction</Typography>
          <Typography variant="body2" color="text.secondary">
            Choose the transaction type, then complete the matching form.
          </Typography>
        </Stack>
      ) : null}
      <Stack spacing={1}>
        <ToggleButtonGroup
          value={transactionType}
          exclusive
          onChange={(_, nextValue: TransactionFormKind | null) => {
            if (nextValue !== null) {
              setTransactionType(nextValue);
            }
          }}
          sx={{ flexWrap: "wrap" }}
        >
          <ToggleButton value="spending">Spending</ToggleButton>
          <ToggleButton value="income">Income</ToggleButton>
          <ToggleButton value="account">Account</ToggleButton>
          <ToggleButton value="fund">Fund</ToggleButton>
        </ToggleButtonGroup>
      </Stack>

      {transactionType === "income" ? (
        <CreateIncomeTransactionForm
          accountingPeriods={accountingPeriods}
          accounts={accounts}
          funds={funds}
          assignmentGoals={assignmentGoals}
          redirectUrl={redirectUrl}
        />
      ) : null}
      {transactionType === "spending" ? (
        <CreateSpendingTransactionForm
          accountingPeriods={accountingPeriods}
          accounts={accounts}
          funds={funds}
          spendingGoals={spendingGoals}
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
    </Stack>
  );
};

export default CreateTransactionForm;
