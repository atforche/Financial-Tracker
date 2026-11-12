"use client";

import type {
  AccountingPeriod,
  ExpectedIncomeSourcesByPeriod,
} from "@/accounting-periods/types";
import { type JSX, useState } from "react";
import type { AccountWithBalance } from "@/accounts/types";
import CreateAccountTransactionForm from "@/transactions/workspace/account/CreateAccountTransactionForm";
import CreateBalanceAdjustmentForm from "@/transactions/workspace/balance-adjustment/CreateBalanceAdjustmentForm";
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
import type { TransactionDetails } from "@/transactions/workspace/TransactionForm";
import { TransactionType } from "@/transactions/types";
import { TransactionTypeProvider } from "@/transactions/workspace/TransactionTypeField";
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
  readonly expectedIncomeSources: ExpectedIncomeSourcesByPeriod;
  readonly locations: Location[];
  readonly redirectUrl: string;
  readonly linkedAccountingPeriodId: string | null;
}

/**
 * Component that displays the form for creating a transaction.
 */
const CreateTransactionForm = function ({
  accountingPeriods,
  accounts,
  funds,
  fundGoals,
  expectedIncomeSources,
  locations,
  redirectUrl,
  linkedAccountingPeriodId,
}: CreateTransactionFormProps): JSX.Element | null {
  const canWrite = useWriteAccess();
  const [transactionType, setTransactionType] = useState<TransactionType>(
    TransactionType.Spending,
  );
  const initialAccountingPeriod =
    accountingPeriods.find(({ id }) => id === linkedAccountingPeriodId) ??
    getDefaultAccountingPeriod(accountingPeriods);
  const [accountingPeriod, setAccountingPeriod] =
    useState<AccountingPeriod | null>(initialAccountingPeriod);
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
      setAccountingPeriod(initialAccountingPeriod);
      setDate(null);
      setDescription("");
    },
  };

  if (!canWrite) {
    return null;
  }

  return (
    <LocationProvider locations={locations}>
      <TransactionTypeProvider
        type={transactionType}
        setType={setTransactionType}
      >
        <PageLayout>
          {transactionType === TransactionType.Income ? (
            <CreateIncomeTransactionForm
              accountingPeriods={accountingPeriods}
              accounts={accounts}
              funds={funds}
              fundGoals={fundGoals}
              expectedIncomeSources={expectedIncomeSources}
              redirectUrl={redirectUrl}
              details={details}
            />
          ) : null}
          {transactionType === TransactionType.Spending ? (
            <CreateSpendingTransactionForm
              accountingPeriods={accountingPeriods}
              accounts={accounts}
              funds={funds}
              fundGoals={fundGoals}
              redirectUrl={redirectUrl}
              details={details}
            />
          ) : null}
          {transactionType === TransactionType.Refund ? (
            <CreateRefundTransactionForm
              accountingPeriods={accountingPeriods}
              accounts={accounts}
              funds={funds}
              fundGoals={fundGoals}
              redirectUrl={redirectUrl}
              details={details}
            />
          ) : null}
          {transactionType === TransactionType.Account ? (
            <CreateAccountTransactionForm
              accountingPeriods={accountingPeriods}
              accounts={accounts}
              redirectUrl={redirectUrl}
              details={details}
            />
          ) : null}
          {transactionType === TransactionType.Fund ? (
            <CreateFundTransactionForm
              accountingPeriods={accountingPeriods}
              funds={funds}
              redirectUrl={redirectUrl}
              details={details}
            />
          ) : null}
          {transactionType === TransactionType.BalanceAdjustment ? (
            <CreateBalanceAdjustmentForm
              accountingPeriods={accountingPeriods}
              accounts={accounts}
              redirectUrl={redirectUrl}
              details={details}
            />
          ) : null}
        </PageLayout>
      </TransactionTypeProvider>
    </LocationProvider>
  );
};

export default CreateTransactionForm;
