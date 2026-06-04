import type { JSX } from "react";
import UpdateGoalForm from "@/goals/UpdateGoalForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the UpdateGoalView component.
 */
interface UpdateGoalViewParams {
  id: string;
}

/**
 * Props for the UpdateGoalView component.
 */
interface UpdateGoalViewProps {
  readonly params: Promise<UpdateGoalViewParams>;
}

/**
 * Component that displays the update goal view.
 */
const UpdateGoalView = async function ({
  params,
}: UpdateGoalViewProps): Promise<JSX.Element> {
  const { id } = await params;

  const apiClient = getApiClient();
  const goalPromise = apiClient.GET("/goals/{goalId}", {
    params: {
      path: {
        goalId: id,
      },
    },
  });

  const { data: goal, error: goalError } = await goalPromise;

  if (typeof goal === "undefined") {
    throw new Error(
      `Failed to fetch goal with ID ${id}: ${goalError.detail ?? "Unknown error"}`,
    );
  }

  const { data: accountingPeriod } = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: goal.accountingPeriodId,
        },
      },
    },
  );

  if (typeof accountingPeriod === "undefined") {
    throw new Error("Failed to fetch goal data");
  }

  return <UpdateGoalForm goal={goal} />;
};

export type { UpdateGoalViewParams };
export default UpdateGoalView;
