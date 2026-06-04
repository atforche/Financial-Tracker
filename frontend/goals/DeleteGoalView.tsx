import DeleteGoalForm from "@/goals/DeleteGoalForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the DeleteGoalView component.
 */
interface DeleteGoalViewParams {
  id: string;
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
  const { id } = await params;

  const apiClient = getApiClient();
  const goalPromise = apiClient.GET("/goals/{goalId}", {
    params: {
      path: {
        goalId: id,
      },
    },
  });

  const [{ data: goal, error: goalError }] = await Promise.all([goalPromise]);

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

  return <DeleteGoalForm accountingPeriod={accountingPeriod} goal={goal} />;
};

export type { DeleteGoalViewParams };
export default DeleteGoalView;
