import type { JSX } from "react";
import UpdateGoalForm from "@/app/accounting-periods/[id]/funds/[fundId]/goal/update/UpdateGoalForm";
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
 * Component that displays the update goal view.
 */
const Page = async function ({ params }: PageProps): Promise<JSX.Element> {
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

export default Page;
