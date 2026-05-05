import DeleteAccountingPeriodForm from "@/accounting-periods/delete/DeleteAccountingPeriodForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the DeleteAccountingPeriodView component.
 */
interface DeleteAccountingPeriodViewParams {
  id: string;
}

/**
 * Props for the DeleteAccountingPeriodView component.
 */
interface DeleteAccountingPeriodViewProps {
  readonly params: Promise<DeleteAccountingPeriodViewParams>;
}

/**
 * Component that displays the delete accounting period view.
 */
const DeleteAccountingPeriodView = async function ({
  params,
}: DeleteAccountingPeriodViewProps): Promise<JSX.Element> {
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

  return <DeleteAccountingPeriodForm accountingPeriod={accountingPeriod} />;
};

export type { DeleteAccountingPeriodViewParams };
export default DeleteAccountingPeriodView;
