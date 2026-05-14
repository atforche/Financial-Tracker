import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountForm from "@/accounts/CreateAccountForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Search parameters for the CreateAccountView component.
 */
interface CreateAccountViewSearchParams {
  accountingPeriodId?: string | null;
}

/**
 * Props for the CreateAccountView component.
 */
interface CreateAccountViewProps {
  readonly searchParams: Promise<CreateAccountViewSearchParams>;
}

/**
 * Component that displays the create account view.
 */
const CreateAccountView = async function ({
  searchParams,
}: CreateAccountViewProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await searchParams;

  const apiClient = getApiClient();
  const { data: accountingPeriods } = await apiClient.GET(
    "/accounting-periods/open",
  );
  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods");
  }

  let routeAccountingPeriod: AccountingPeriod | null = null;
  if (typeof accountingPeriodId === "string") {
    routeAccountingPeriod =
      accountingPeriods.find((period) => period.id === accountingPeriodId) ??
      null;
  }

  return (
    <CreateAccountForm
      accountingPeriods={accountingPeriods}
      routeAccountingPeriod={routeAccountingPeriod}
    />
  );
};

export type { CreateAccountViewSearchParams };
export default CreateAccountView;
