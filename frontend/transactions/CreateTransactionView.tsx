import CreateTransactionForm from "@/transactions/CreateTransactionForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Search parameters for the CreateTransactionView component.
 */
interface CreateTransactionViewSearchParams {
  accountingPeriodId?: string;
  debitAccountId?: string;
  creditAccountId?: string;
  debitFundId?: string;
  creditFundId?: string;
}

/**
 * Props for the CreateTransactionView component.
 */
interface CreateTransactionViewProps {
  readonly searchParams: Promise<CreateTransactionViewSearchParams>;
}

/**
 * Component that displays the create transaction view.
 */
const CreateTransactionView = async function ({
  searchParams,
}: CreateTransactionViewProps): Promise<JSX.Element> {
  const {
    accountingPeriodId,
    debitAccountId,
    creditAccountId,
    debitFundId,
    creditFundId,
  } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods/open");
  const accountsPromise = apiClient.GET("/accounts");
  const fundsPromise = apiClient.GET("/funds");

  const [{ data: accountingPeriods }, { data: accounts }, { data: funds }] =
    await Promise.all([
      accountingPeriodsPromise,
      accountsPromise,
      fundsPromise,
    ]);

  if (
    typeof accountingPeriods === "undefined" ||
    typeof accounts === "undefined" ||
    typeof funds === "undefined"
  ) {
    throw new Error("Failed to fetch data");
  }

  const routeAccountingPeriod =
    accountingPeriods.find((ap) => ap.id === accountingPeriodId) ?? null;
  const routeDebitAccount =
    accounts.items.find((a) => a.id === debitAccountId) ?? null;
  const routeCreditAccount =
    accounts.items.find((a) => a.id === creditAccountId) ?? null;
  const routeDebitFund = funds.items.find((f) => f.id === debitFundId) ?? null;
  const routeCreditFund =
    funds.items.find((f) => f.id === creditFundId) ?? null;

  return (
    <CreateTransactionForm
      accountingPeriods={accountingPeriods}
      accounts={accounts.items}
      funds={funds.items}
      routeAccountingPeriod={routeAccountingPeriod}
      routeDebitAccount={routeDebitAccount}
      routeCreditAccount={routeCreditAccount}
      routeDebitFund={routeDebitFund}
      routeCreditFund={routeCreditFund}
    />
  );
};

export type { CreateTransactionViewSearchParams };
export default CreateTransactionView;
