import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import CreateAccountForm from "@/app/accounts//create/CreateAccountForm";
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
 * Component that displays the create account view.
 */
const Page = async function ({
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await searchParams;
  const apiClient = getApiClient();

  const accountingPeriodPromise = apiClient.GET("/accounting-periods/open");
  const fundsPromise = apiClient.GET("/funds");
  const [
    { data: accountingPeriodData },
    { data: fundsData, error: fundsError },
  ] = await Promise.all([accountingPeriodPromise, fundsPromise]);

  if (
    typeof accountingPeriodData === "undefined" ||
    typeof fundsData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch accounting periods or funds: ${fundsError?.detail ?? "Unknown error"}`,
    );
  }

  let providedAccountingPeriod: AccountingPeriod | null = null;
  if (accountingPeriodId !== null) {
    providedAccountingPeriod =
      accountingPeriodData.find((period) => period.id === accountingPeriodId) ??
      null;
  }

  return (
    <CreateAccountForm
      accountingPeriods={accountingPeriodData}
      funds={fundsData.items.map((fund) => ({ id: fund.id, name: fund.name }))}
      providedAccountingPeriod={providedAccountingPeriod}
    />
  );
};

export default Page;
