import CreateTransactionForm from "@/app/transactions/create/CreateTransactionForm";
import type { CreateTransactionFormSearchParams } from "@/app/transactions/create/createTransactionFormSearchParams";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly searchParams: Promise<CreateTransactionFormSearchParams>;
}

/**
 * Component that displays the create transaction view.
 */
const Page = async function (props: PageProps): Promise<JSX.Element> {
  const searchParams = await props.searchParams;
  const apiClient = getApiClient();

  const accountingPeriodsPromise = apiClient.GET("/accounting-periods/open");
  const fundsPromise = apiClient.GET("/funds");
  const accountsPromise = apiClient.GET("/accounts");

  const [
    { data: accountingPeriodsData },
    { data: fundsData, error: fundsError },
    { data: accountsData, error: accountsError },
  ] = await Promise.all([
    accountingPeriodsPromise,
    fundsPromise,
    accountsPromise,
  ]);

  if (
    typeof accountingPeriodsData === "undefined" ||
    typeof fundsData === "undefined" ||
    typeof accountsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch data for create transaction page: ${fundsError?.detail ?? accountsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <CreateTransactionForm
      searchParams={searchParams}
      accountingPeriods={accountingPeriodsData}
      funds={fundsData.items.map((fund) => ({ id: fund.id, name: fund.name }))}
      accounts={accountsData.items.map((account) => ({
        id: account.id,
        name: account.name,
      }))}
    />
  );
};

export default Page;
