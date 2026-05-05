import DeleteFundForm from "@/funds/delete/DeleteFundForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the DeleteFundView component.
 */
interface DeleteFundViewParams {
  id: string;
}

/**
 * Search parameters for the DeleteFundView component.
 */
interface DeleteFundViewSearchParams {
  accountingPeriodId?: string | null;
}

/**
 * Props for the DeleteFundView component.
 */
interface DeleteFundViewProps {
  readonly params: Promise<DeleteFundViewParams>;
  readonly searchParams: Promise<DeleteFundViewSearchParams>;
}

/**
 * Component that displays the form for deleting a fund.
 */
const DeleteFundView = async function ({
  params,
  searchParams,
}: DeleteFundViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId } = await searchParams;

  const apiClient = getApiClient();
  const fundPromise = apiClient.GET("/funds/{fundId}", {
    params: {
      path: {
        fundId: id,
      },
    },
  });
  const accountingPeriodPromise =
    typeof accountingPeriodId !== "undefined" && accountingPeriodId !== null
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: {
            path: {
              accountingPeriodId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });

  const [{ data: fund, error: fundError }, { data: accountingPeriod }] =
    await Promise.all([fundPromise, accountingPeriodPromise]);

  if (typeof fund === "undefined") {
    throw new Error(`Failed to fetch fund data: ${fundError.detail}`);
  }

  return (
    <DeleteFundForm fund={fund} accountingPeriod={accountingPeriod ?? null} />
  );
};

export type { DeleteFundViewParams, DeleteFundViewSearchParams };
export default DeleteFundView;
