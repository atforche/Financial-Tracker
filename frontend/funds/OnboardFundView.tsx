import type { JSX } from "react";
import OnboardFundForm from "@/funds/OnboardFundForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Component that displays the fund onboarding view.
 */
const OnboardFundView = async function (): Promise<JSX.Element> {
  const apiClient = getApiClient();
  const { data: unassignedFund } = await apiClient.GET("/funds/unassigned");

  return (
    <OnboardFundForm
      unassignedBalance={unassignedFund?.currentBalance.postedBalance ?? null}
    />
  );
};

export default OnboardFundView;
