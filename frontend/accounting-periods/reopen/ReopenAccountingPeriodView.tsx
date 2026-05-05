import type { JSX } from "react";
import ReopenAccountingPeriodForm from "@/accounting-periods/reopen/ReopenAccountingPeriodForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the ReopenAccountingPeriodView component.
 */
interface ReopenAccountingPeriodViewParams {
  id: string;
}

/**
 * Props for the ReopenAccountingPeriodView component.
 */
interface ReopenAccountingPeriodViewProps {
  readonly params: Promise<ReopenAccountingPeriodViewParams>;
}

/**
 * Component that displays the reopen accounting period view.
 */
const ReopenAccountingPeriodView = async function ({
  params,
}: ReopenAccountingPeriodViewProps): Promise<JSX.Element> {
  const { id } = await params;

  const apiClient = getApiClient();
  const { data: accountingPeriod, error } = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );

  if (typeof accountingPeriod === "undefined") {
    throw new Error(`Failed to fetch accounting period data: ${error.detail}`);
  }

  return <ReopenAccountingPeriodForm accountingPeriod={accountingPeriod} />;
};

export type { ReopenAccountingPeriodViewParams };
export default ReopenAccountingPeriodView;
