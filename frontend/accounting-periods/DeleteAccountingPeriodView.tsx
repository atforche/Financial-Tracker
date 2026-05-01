import DeleteAccountingPeriodForm from "@/accounting-periods/DeleteAccountingPeriodForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the DeleteAccountingPeriodView component.
 */
interface DeleteAccountingPeriodViewProps {
  readonly params: Promise<{
    id: string;
  }>;
}

/**
 * Component that displays the delete accounting period view.
 */
const DeleteAccountingPeriodView = async function ({
  params,
}: DeleteAccountingPeriodViewProps): Promise<JSX.Element> {
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

export default DeleteAccountingPeriodView;
