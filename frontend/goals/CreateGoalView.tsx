import CreateGoalForm from "@/goals/CreateGoalForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the CreateGoalView component.
 */
interface CreateGoalViewParams {
  id: string;
  fundId: string;
}

/**
 * Props for the CreateGoalView component.
 */
interface CreateGoalViewProps {
  readonly params: Promise<CreateGoalViewParams>;
}

/**
 * Component that displays the create goal view.
 */
const CreateGoalView = async function ({
  params,
}: CreateGoalViewProps): Promise<JSX.Element> {
  const { id, fundId } = await params;

  const apiClient = getApiClient();
  const [{ data: fund, error: fundError }, { data: accountingPeriod }] =
    await Promise.all([
      apiClient.GET("/funds/{fundId}", {
        params: {
          path: {
            fundId,
          },
        },
      }),
      apiClient.GET("/accounting-periods/{accountingPeriodId}", {
        params: {
          path: {
            accountingPeriodId: id,
          },
        },
      }),
    ]);

  if (typeof fund === "undefined" || typeof accountingPeriod === "undefined") {
    throw new Error(
      `Failed to fetch create goal data: ${fundError?.detail ?? "Unknown error"}`,
    );
  }

  return <CreateGoalForm fund={fund} accountingPeriod={accountingPeriod} />;
};

export type { CreateGoalViewParams };
export default CreateGoalView;
