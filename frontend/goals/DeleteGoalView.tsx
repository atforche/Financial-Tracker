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
 * Search parameters for the DeleteGoalView component.
 */
interface DeleteGoalViewSearchParams {
  fundId?: string | null;
}

/**
 * Props for the DeleteGoalView component.
 */
interface DeleteGoalViewProps {
  readonly params: Promise<DeleteGoalViewParams>;
  readonly searchParams: Promise<DeleteGoalViewSearchParams>;
}

/**
 * Component that displays the delete goal view.
 */
const DeleteGoalView = async function ({
  params,
  searchParams,
}: DeleteGoalViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { fundId } = await searchParams;

  const apiClient = getApiClient();
  const goalPromise = apiClient.GET("/goals/{goalId}", {
    params: {
      path: {
        goalId: id,
      },
    },
  });
  const fundPromise =
    typeof fundId === "string"
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId,
            },
          },
        })
      : Promise.resolve({ data: null });

  const [{ data: goal, error: goalError }, { data: fund }] = await Promise.all([
    goalPromise,
    fundPromise,
  ]);

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

  return (
    <DeleteGoalForm
      accountingPeriod={accountingPeriod}
      goal={goal}
      fund={fund ?? null}
    />
  );
};

export type { DeleteGoalViewParams, DeleteGoalViewSearchParams };
export default DeleteGoalView;
