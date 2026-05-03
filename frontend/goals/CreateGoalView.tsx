import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateGoalForm from "@/goals/CreateGoalForm";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Search parameters for the CreateGoalView component.
 */
interface CreateGoalViewSearchParams {
  accountingPeriodId?: string | null;
  fundId?: string | null;
}

/**
 * Props for the CreateGoalView component.
 */
interface CreateGoalViewProps {
  readonly searchParams: Promise<CreateGoalViewSearchParams>;
}

/**
 * Component that displays the create goal view.
 */
const CreateGoalView = async function ({
  searchParams,
}: CreateGoalViewProps): Promise<JSX.Element> {
  const { accountingPeriodId, fundId } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodsPromise = apiClient.GET("/accounting-periods/open");
  const fundsPromise = apiClient.GET("/funds");

  const [{ data: accountingPeriods }, { data: funds }] = await Promise.all([
    accountingPeriodsPromise,
    fundsPromise,
  ]);

  if (
    typeof accountingPeriods === "undefined" ||
    typeof funds === "undefined"
  ) {
    throw new Error("Failed to fetch accounting periods or funds");
  }

  let providedAccountingPeriod: AccountingPeriod | null = null;
  if (typeof accountingPeriodId === "string") {
    providedAccountingPeriod =
      accountingPeriods.find((period) => period.id === accountingPeriodId) ??
      null;
  }

  let providedFund: Fund | null = null;
  if (typeof fundId === "string") {
    providedFund = funds.items.find((fund) => fund.id === fundId) ?? null;
  }

  return (
    <CreateGoalForm
      accountingPeriods={accountingPeriods}
      funds={funds.items}
      providedAccountingPeriod={providedAccountingPeriod}
      providedFund={providedFund}
    />
  );
};

export type { CreateGoalViewSearchParams };
export default CreateGoalView;
