import type { JSX } from "react";
import UpdateGoalForm from "@/goals/update/UpdateGoalForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the UpdateGoalView component.
 */
interface UpdateGoalViewParams {
  id: string;
}

/**
 * Search parameters for the UpdateGoalView component.
 */
interface UpdateGoalViewSearchParams {
  fundId?: string | null;
}

/**
 * Props for the UpdateGoalView component.
 */
interface UpdateGoalViewProps {
  readonly params: Promise<UpdateGoalViewParams>;
  readonly searchParams: Promise<UpdateGoalViewSearchParams>;
}

/**
 * Component that displays the update goal view.
 */
const UpdateGoalView = async function ({
  params,
  searchParams,
}: UpdateGoalViewProps): Promise<JSX.Element> {
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
    <UpdateGoalForm
      accountingPeriod={accountingPeriod}
      goal={goal}
      fund={fund ?? null}
    />
  );
};

export type { UpdateGoalViewParams, UpdateGoalViewSearchParams };
export default UpdateGoalView;
