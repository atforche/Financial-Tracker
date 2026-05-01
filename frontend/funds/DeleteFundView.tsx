import DeleteFundForm from "@/funds/DeleteFundForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the DeleteFundView Component.
 */
interface DeleteFundViewProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    accountingPeriodId?: string | null;
  }>;
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

export default DeleteFundView;
