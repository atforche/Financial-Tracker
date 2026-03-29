import type { AccountingPeriod } from "@/data/accountingPeriodTypes";
import CreateFundForm from "@/app/funds/create/CreateFundForm";
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
 * Component that displays the create fund view.
 */
const Page = async function ({
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await searchParams;
  const apiClient = getApiClient();

  const { data: accountingPeriodData } = await apiClient.GET(
    "/accounting-periods/open",
  );

  if (typeof accountingPeriodData === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  let providedAccountingPeriod: AccountingPeriod | null = null;
  if (accountingPeriodId !== null) {
    providedAccountingPeriod =
      accountingPeriodData.find((period) => period.id === accountingPeriodId) ??
      null;
  }

  return (
    <CreateFundForm
      accountingPeriods={accountingPeriodData}
      providedAccountingPeriod={providedAccountingPeriod}
    />
  );
};

export default Page;
