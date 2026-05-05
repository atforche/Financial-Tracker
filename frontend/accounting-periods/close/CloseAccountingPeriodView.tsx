import CloseAccountingPeriodForm from "@/accounting-periods/close/CloseAccountingPeriodForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the CloseAccountingPeriodView component.
 */
interface CloseAccountingPeriodViewParams {
  id: string;
}

/**
 * Props for the CloseAccountingPeriodView component.
 */
interface CloseAccountingPeriodViewProps {
  readonly params: Promise<CloseAccountingPeriodViewParams>;
}

/**
 * Component that displays the close accounting period view.
 */
const CloseAccountingPeriodView = async function ({
  params,
}: CloseAccountingPeriodViewProps): Promise<JSX.Element> {
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

  return <CloseAccountingPeriodForm accountingPeriod={accountingPeriod} />;
};

export type { CloseAccountingPeriodViewParams };
export default CloseAccountingPeriodView;
