import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateFundForm from "@/funds/CreateFundForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Search parameters for the CreateFundView component.
 */
interface CreateFundViewSearchParams {
  accountingPeriodId?: string | null;
}

/**
 * Props for the CreateFundView component.
 */
interface CreateFundViewProps {
  readonly searchParams: Promise<CreateFundViewSearchParams>;
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

export type { CreateFundViewSearchParams };
export default CreateFundView;
