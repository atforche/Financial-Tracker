import type { JSX } from "react";
import UpdateGoalForm from "@/goals/UpdateGoalForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the UpdateGoalView component.
 */
interface UpdateGoalViewProps {
  readonly params: Promise<{
    id: string;
    fundId: string;
  }>;
}

/**
 * Component that displays the update goal view.
 */
const UpdateGoalView = async function ({
  params,
}: UpdateGoalViewProps): Promise<JSX.Element> {
  const { id, fundId } = await params;
  const apiClient = getApiClient();
  const [{ data: fundData }, { data: goalData }] = await Promise.all([
    apiClient.GET("/funds/{fundId}", {
      params: {
        path: {
          fundId,
        },
      },
    }),
    apiClient.GET("/goals", {
      body: {
        fundId,
        accountingPeriodId: id,
      },
    }),
  ]);

  if (typeof fundData === "undefined" || typeof goalData === "undefined") {
    throw new Error("Failed to fetch goal data");
  }

  return <UpdateGoalForm fund={fundData} goal={goalData} />;
};

export default UpdateGoalView;
