import type { AccountingPeriod } from "@/accounting-periods/types";
import CreateAccountForm from "@/accounts/CreateAccountForm";
import type { FundIdentifier } from "@/funds/types";
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
 * Gets all fund identifiers available to the user.
 */
const getFundIdentifiers = async function (): Promise<FundIdentifier[]> {
  const apiClient = getApiClient();
  const { data: funds } = await apiClient.GET("/funds", {
    params: {
      query: {
        Search: "",
        Sort: null,
      },
    },
  });
  if (typeof funds === "undefined") {
    throw new Error(`Failed to fetch funds`);
  }

  let fundIdentifiers = funds.items;
  if (funds.totalCount > funds.items.length) {
    const { data: allFunds } = await apiClient.GET("/funds", {
      params: {
        query: {
          Search: "",
          Sort: null,
          Limit: funds.totalCount,
          Offset: 0,
        },
      },
    });
    if (typeof allFunds === "undefined") {
      throw new Error(`Failed to fetch all funds`);
    }
    fundIdentifiers = allFunds.items;
  }

  return fundIdentifiers.map((fund) => ({
    id: fund.id,
    name: fund.name,
  }));
};

/**
 * Component that displays the create account view.
 */
const CreateAccountView = async function ({
  searchParams,
}: CreateAccountViewProps): Promise<JSX.Element> {
  const { accountingPeriodId } = await searchParams;
  const apiClient = getApiClient();

  const [accountingPeriodsResult, funds] = await Promise.all([
    apiClient.GET("/accounting-periods/open"),
    getFundIdentifiers(),
  ]);
  const { data: accountingPeriods } = accountingPeriodsResult;

  if (typeof accountingPeriods === "undefined") {
    throw new Error("Failed to fetch accounting periods:");
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
      funds={funds}
      providedAccountingPeriod={providedAccountingPeriod}
    />
  );
};

export type { CreateAccountViewSearchParams };
export default CreateAccountView;
