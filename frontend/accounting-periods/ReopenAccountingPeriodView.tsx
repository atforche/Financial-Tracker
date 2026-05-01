import type { JSX } from "react";
import ReopenAccountingPeriodForm from "@/accounting-periods/ReopenAccountingPeriodForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the ReopenAccountingPeriodView component.
 */
interface ReopenAccountingPeriodViewProps {
  readonly params: Promise<{
    id: string;
  }>;
}

/**
 * Component that displays the reopen accounting period view.
 */
const ReopenAccountingPeriodView = async function ({
  params,
}: ReopenAccountingPeriodViewProps): Promise<JSX.Element> {
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

  return <ReopenAccountingPeriodForm accountingPeriod={data} />;
};

export default ReopenAccountingPeriodView;
