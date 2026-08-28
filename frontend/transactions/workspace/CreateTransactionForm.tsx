"use client";

import { type JSX, useState } from "react";
import { Stack, Typography } from "@mui/material";
import type { AccountWithBalance } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountTransactionForm from "@/transactions/workspace/account/CreateAccountTransactionForm";
import CreateFundTransactionForm from "@/transactions/workspace/fund/CreateFundTransactionForm";
import CreateIncomeTransactionForm from "@/transactions/workspace/income/CreateIncomeTransactionForm";
import CreateRefundTransactionForm from "@/transactions/workspace/refund/CreateRefundTransactionForm";
import CreateSpendingTransactionForm from "@/transactions/workspace/spending/CreateSpendingTransactionForm";
import type { Dayjs } from "dayjs";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { FundWithBalance } from "@/funds/types";
import type { Location } from "@/locations/types";
import { LocationProvider } from "@/locations/LocationProvider";
import PageLayout from "@/framework/view/PageLayout";
import ToggleButtonSelector from "@/framework/forms/ToggleButtonSelector";
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { getDefaultAccountingPeriod } from "@/transactions/workspace/helpers";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the CreateTransactionForm component.
 */
interface CreateTransactionFormProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly accounts: AccountWithBalance[];
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly locations: Location[];
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
  locations,
  redirectUrl,
  showHeading = true,
}: CreateTransactionFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  type TransactionFormKind =
    "spending" | "refund" | "income" | "account" | "fund";
  const [transactionType, setTransactionType] =
    useState<TransactionFormKind>("spending");
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(
      getDefaultAccountingPeriod(accountingPeriods),
    );
  const [date, setDate] = useState<Dayjs | null>(null);
  const [description, setDescription] = useState("");
  const details: TransactionDetails = {
    accountingPeriod,
    setAccountingPeriod,
    date,
    setDate,
    description,
    setDescription,
    reset: (): void => {
      setAccountingPeriod(getDefaultAccountingPeriod(accountingPeriods));
      setDate(null);
      setDescription("");
    },
  };

  if (!canWrite) {
    return null;
  }

  return (
    <LocationProvider locations={locations}>
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
              { value: "refund", label: "Refund" },
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
            details={details}
          />
        ) : null}
        {transactionType === "spending" ? (
          <CreateSpendingTransactionForm
            accountingPeriods={accountingPeriods}
            accounts={accounts}
            funds={funds}
            fundGoals={fundGoals}
            redirectUrl={redirectUrl}
            details={details}
          />
        ) : null}
        {transactionType === "refund" ? (
          <CreateRefundTransactionForm
            accountingPeriods={accountingPeriods}
            accounts={accounts}
            funds={funds}
            fundGoals={fundGoals}
            redirectUrl={redirectUrl}
            details={details}
          />
        ) : null}
        {transactionType === "account" ? (
          <CreateAccountTransactionForm
            accountingPeriods={accountingPeriods}
            accounts={accounts}
            redirectUrl={redirectUrl}
            details={details}
          />
        ) : null}
        {transactionType === "fund" ? (
          <CreateFundTransactionForm
            accountingPeriods={accountingPeriods}
            funds={funds}
            redirectUrl={redirectUrl}
            details={details}
          />
        ) : null}
      </PageLayout>
    </LocationProvider>
  );
};

export default CreateTransactionForm;
