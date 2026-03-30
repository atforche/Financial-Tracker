import CloseAccountingPeriodForm from "@/app/accounting-periods/[id]/close/CloseAccountingPeriodForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
}

/**
 * Component that displays the close accounting period view.
 */
const Page = async function ({ params }: PageProps): Promise<JSX.Element> {
  const { id } = await params;

  const apiClient = getApiClient();
  const { data, error } = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );

  if (typeof data === "undefined") {
    throw new Error(`Failed to fetch accounting period data: ${error.detail}`);
  }

  return <CloseAccountingPeriodForm accountingPeriod={data} />;
};

export default Page;
