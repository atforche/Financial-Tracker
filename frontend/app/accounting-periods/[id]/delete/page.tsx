import DeleteAccountingPeriodForm from "@/app/accounting-periods/[id]/delete/DeleteAccountingPeriodForm";
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
 * Component that displays the delete accounting period view.
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

  return <DeleteAccountingPeriodForm accountingPeriod={data} />;
};

export default Page;
