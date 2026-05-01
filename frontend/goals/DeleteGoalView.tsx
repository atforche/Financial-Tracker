import DeleteGoalForm from "@/goals/DeleteGoalForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the DeleteGoalView component.
 */
interface DeleteGoalViewParams {
  id: string;
  fundId: string;
}

/**
 * Props for the DeleteGoalView component.
 */
interface DeleteGoalViewProps {
  readonly params: Promise<DeleteGoalViewParams>;
}

/**
 * Component that displays the delete goal view.
 */
const DeleteGoalView = async function ({
  params,
}: DeleteGoalViewProps): Promise<JSX.Element> {
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

  return <DeleteGoalForm fund={fundData} goal={goalData} />;
};

export type { DeleteGoalViewParams };
export default DeleteGoalView;
