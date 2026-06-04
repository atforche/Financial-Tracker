import CreateGoalForm from "@/goals/CreateGoalForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Component that displays the create goal view.
 */
const CreateGoalView = async function (): Promise<JSX.Element> {
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

  return (
    <CreateGoalForm accountingPeriods={accountingPeriods} funds={funds.items} />
  );
};

export default CreateGoalView;
