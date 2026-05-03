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

  return (
    <CreateAccountForm
      accountingPeriods={accountingPeriods}
      funds={funds.items}
      providedAccountingPeriod={providedAccountingPeriod}
    />
  );
};

export type { CreateAccountViewSearchParams };
export default CreateAccountView;
