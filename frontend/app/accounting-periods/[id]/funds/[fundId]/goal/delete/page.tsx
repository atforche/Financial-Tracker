import DeleteFundGoalForm from "@/app/accounting-periods/[id]/funds/[fundId]/goal/delete/DeleteFundGoalForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the page component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
    fundId: string;
  }>;
}

/**
 * Component that displays the delete fund goal view.
 */
const Page = async function ({ params }: PageProps): Promise<JSX.Element> {
  const { id, fundId } = await params;
  const apiClient = getApiClient();
  const [{ data: fundData }, { data: fundGoalData }] = await Promise.all([
    apiClient.GET("/funds/{fundId}", {
      params: {
        path: {
          fundId,
        },
      },
    }),
    apiClient.GET("/funds/{fundId}/goals/{accountingPeriodId}", {
      params: {
        path: {
          fundId,
          accountingPeriodId: id,
        },
      },
    }),
  ]);

  if (typeof fundData === "undefined" || typeof fundGoalData === "undefined") {
    throw new Error("Failed to fetch fund goal data");
  }

  return <DeleteFundGoalForm fund={fundData} fundGoal={fundGoalData} />;
};

export default Page;
