import CreateGoalForm from "@/app/accounting-periods/[id]/funds/[fundId]/goal/create/CreateGoalForm";
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
 * Component that displays the create goal view.
 */
const Page = async function ({ params }: PageProps): Promise<JSX.Element> {
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

export default Page;
