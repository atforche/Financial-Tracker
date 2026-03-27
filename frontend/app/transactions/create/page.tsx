import CreateTransactionForm from "@/app/transactions/create/CreateTransactionForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly searchParams: Promise<{
    accountingPeriodId?: string;
    debitAccountId?: string;
    creditAccountId?: string;
  }>;
}

/**
 * Component that displays the create transaction view.
 */
const Page = async function (props: PageProps): Promise<JSX.Element> {
  const { accountingPeriodId, debitAccountId, creditAccountId } =
    await props.searchParams;
  const apiClient = getApiClient();

  const accountingPeriodsPromise = apiClient.GET("/accounting-periods/open");
  const fundsPromise = apiClient.GET("/funds");
  const accountsPromise = apiClient.GET("/accounts");
  const accountingPeriodPromise =
    typeof accountingPeriodId !== "undefined"
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: {
            path: {
              accountingPeriodId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });
  const debitAccountPromise =
    typeof debitAccountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId: debitAccountId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });
  const creditAccountPromise =
    typeof creditAccountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId: creditAccountId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });

  const [
    { data: accountingPeriodsData },
    { data: fundsData, error: fundsError },
    { data: accountsData, error: accountsError },
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: debitAccountData, error: debitAccountError },
    { data: creditAccountData, error: creditAccountError },
  ] = await Promise.all([
    accountingPeriodsPromise,
    fundsPromise,
    accountsPromise,
    accountingPeriodPromise,
    debitAccountPromise,
    creditAccountPromise,
  ]);

  if (
    typeof accountingPeriodsData === "undefined" ||
    typeof fundsData === "undefined" ||
    typeof accountsData === "undefined" ||
    typeof accountingPeriodData === "undefined" ||
    typeof debitAccountData === "undefined" ||
    typeof creditAccountData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch data for create transaction page: ${fundsError?.detail ?? accountsError?.detail ?? accountingPeriodError?.detail ?? debitAccountError?.detail ?? creditAccountError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <CreateTransactionForm
      accountingPeriods={accountingPeriodsData}
      funds={fundsData.items.map((fund) => ({ id: fund.id, name: fund.name }))}
      accounts={accountsData.items.map((account) => ({
        id: account.id,
        name: account.name,
      }))}
      providedAccountingPeriod={accountingPeriodData}
      providedDebitAccount={debitAccountData}
      providedCreditAccount={creditAccountData}
    />
  );
};

export default Page;
