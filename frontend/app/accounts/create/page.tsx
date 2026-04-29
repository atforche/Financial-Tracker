import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import CreateAccountForm from "@/app/accounts/create/CreateAccountForm";
import type { FundIdentifier } from "@/data/fundTypes";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page component.
 */
interface PageProps {
  readonly searchParams: Promise<{
    accountingPeriodId?: string | null;
  }>;
}

/**
 * Gets all fund identifiers available to the user.
 */
const getFundIdentifiers = async function (): Promise<FundIdentifier[]> {
  const apiClient = getApiClient();
  const { data } = await apiClient.GET("/funds", {
    params: {
      query: {
        Search: "",
        Sort: null,
      },
    },
  });
  if (typeof data === "undefined") {
    throw new Error(`Failed to fetch funds`);
  }

  let funds = data.items;
  if (data.totalCount > data.items.length) {
    const { data: allFunds } = await apiClient.GET("/funds", {
      params: {
        query: {
          Search: "",
          Sort: null,
          Limit: data.totalCount,
          Offset: 0,
        },
      },
    });
    if (typeof allFunds === "undefined") {
      throw new Error(`Failed to fetch all funds`);
    }
    funds = allFunds.items;
  }

  return funds.map((fund) => ({
    id: fund.id,
    name: fund.name,
  }));
};

/**
 * Component that displays the create account view.
 */
const Page = async function ({
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await searchParams;
  const apiClient = getApiClient();

  const [accountingPeriodsResult, funds] = await Promise.all([
    apiClient.GET("/accounting-periods/open"),
    getFundIdentifiers(),
  ]);
  const { data: accountingPeriodData } = accountingPeriodsResult;

  if (typeof accountingPeriodData === "undefined") {
    throw new Error("Failed to fetch accounting periods:");
  }

  let providedAccountingPeriod: AccountingPeriod | null = null;
  if (typeof accountingPeriodId === "string") {
    providedAccountingPeriod =
      accountingPeriodData.find((period) => period.id === accountingPeriodId) ??
      null;
  }

  return (
    <CreateAccountForm
      accountingPeriods={accountingPeriodData}
      funds={funds}
      providedAccountingPeriod={providedAccountingPeriod}
    />
  );
};

export default Page;
