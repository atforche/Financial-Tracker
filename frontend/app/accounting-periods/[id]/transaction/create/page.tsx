import CreateTransactionForm from "@/app/accounting-periods/[id]/transaction/create/CreateTransactionForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Component that displays the create transaction view.
 */
const Page = async function (props: {
  readonly params: Promise<{ id: string }>;
}): Promise<JSX.Element> {
  const { id } = await props.params;
  const apiClient = getApiClient();

  const accountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );
  const fundsPromise = apiClient.GET("/funds");
  const accountsPromise = apiClient.GET("/accounts");
  const [
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: fundsData, error: fundsError },
    { data: accountsData, error: accountsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundsPromise,
    accountsPromise,
  ]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof fundsData === "undefined" ||
    typeof accountsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${accountingPeriodError?.detail ?? fundsError?.detail ?? accountsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <CreateTransactionForm
      accountingPeriod={accountingPeriodData}
      funds={fundsData.items.map((fund) => ({ id: fund.id, name: fund.name }))}
      accounts={accountsData.items.map((account) => ({
        id: account.id,
        name: account.name,
      }))}
    />
  );
};

export default Page;
