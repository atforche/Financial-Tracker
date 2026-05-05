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

  const { data: accountingPeriods } = await apiClient.GET(
    "/accounting-periods/open",
  );

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  let routeAccountingPeriod: AccountingPeriod | null = null;
  if (accountingPeriodId !== null) {
    routeAccountingPeriod =
      accountingPeriods.find((period) => period.id === accountingPeriodId) ??
      null;
  }

  return (
    <CreateFundForm
      accountingPeriods={accountingPeriods}
      routeAccountingPeriod={routeAccountingPeriod}
    />
  );
};

export type { CreateFundViewSearchParams };
export default CreateFundView;
