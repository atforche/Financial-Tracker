import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateFundForm from "@/funds/CreateFundForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the CreateFundView component.
 */
interface CreateFundViewProps {
  readonly searchParams: Promise<{
    accountingPeriodId?: string | null;
  }>;
}

/**
 * Component that displays the create fund view.
 */
const CreateFundView = async function ({
  searchParams,
}: CreateFundViewProps): Promise<JSX.Element> {
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

export default CreateFundView;
