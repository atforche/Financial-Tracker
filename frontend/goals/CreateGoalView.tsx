import CreateGoalForm from "@/goals/CreateGoalForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the CreateGoalView component.
 */
interface CreateGoalViewProps {
  readonly params: Promise<{
    id: string;
    fundId: string;
  }>;
}

/**
 * Component that displays the create goal view.
 */
const CreateGoalView = async function ({
  params,
}: CreateGoalViewProps): Promise<JSX.Element> {
  const { id, fundId } = await params;

  const apiClient = getApiClient();
  const [{ data: fundData, error: fundError }, { data: accountingPeriodData }] =
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

  if (
    typeof fundData === "undefined" ||
    typeof accountingPeriodData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch create goal data: ${fundError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <CreateGoalForm fund={fundData} accountingPeriod={accountingPeriodData} />
  );
};

export default CreateGoalView;
