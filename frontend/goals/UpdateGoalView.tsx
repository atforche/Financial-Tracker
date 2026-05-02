import type { JSX } from "react";
import UpdateGoalForm from "@/goals/UpdateGoalForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the UpdateGoalView component.
 */
interface UpdateGoalViewParams {
  id: string;
  fundId: string;
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
  const { id, fundId } = await params;
  const apiClient = getApiClient();
  const [{ data: accountingPeriod }, { data: fund }, { data: goal }] =
    await Promise.all([
      apiClient.GET("/accounting-periods/{accountingPeriodId}", {
        params: {
          path: {
            accountingPeriodId: id,
          },
        },
      }),
      apiClient.GET("/funds/{fundId}", {
        params: {
          path: {
            fundId,
          },
        },
      }),
      apiClient.GET("/goals", {
        params: {
          query: {
            fundId,
            AccountingPeriodId: id,
          },
        },
      }),
    ]);

  if (
    typeof accountingPeriod === "undefined" ||
    typeof fund === "undefined" ||
    typeof goal === "undefined"
  ) {
    throw new Error("Failed to fetch goal data");
  }

  return (
    <UpdateGoalForm
      accountingPeriod={accountingPeriod}
      fund={fund}
      goal={goal}
    />
  );
};

export type { UpdateGoalViewParams };
export default UpdateGoalView;
