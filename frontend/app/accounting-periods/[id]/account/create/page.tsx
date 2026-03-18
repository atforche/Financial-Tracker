import CreateAccountForm from "@/app/accounting-periods/[id]/account/create/CreateAccountForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Component that displays the create account view.
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
  const [
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: fundsData, error: fundsError },
  ] = await Promise.all([accountingPeriodPromise, fundsPromise]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof fundsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch accounting period with ID ${id}: ${accountingPeriodError?.detail ?? fundsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <CreateAccountForm
      accountingPeriod={accountingPeriodData}
      funds={fundsData.items.map((fund) => ({ id: fund.id, name: fund.name }))}
    />
  );
};

export default Page;
