import type { JSX } from "react";
import UpdateFundForm from "@/funds/update/UpdateFundForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the UpdateFundView component.
 */
interface UpdateFundViewParams {
  id: string;
}

/**
 * Search parameters for the UpdateFundView component.
 */
interface UpdateFundViewSearchParams {
  accountingPeriodId?: string | null;
}

/**
 * Props for the UpdateFundView component.
 */
interface UpdateFundViewProps {
  readonly params: Promise<UpdateFundViewParams>;
  readonly searchParams: Promise<UpdateFundViewSearchParams>;
}

/**
 * Component that displays the form for updating a fund.
 */
const UpdateFundView = async function ({
  params,
  searchParams,
}: UpdateFundViewProps): Promise<JSX.Element> {
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
      : Promise.resolve({ data: null });

  const [{ data: fund }, { data: accountingPeriod }] = await Promise.all([
    fundPromise,
    accountingPeriodPromise,
  ]);

  if (typeof fund === "undefined") {
    throw new Error("Failed to fetch fund data");
  }

  return (
    <UpdateFundForm
      fund={fund}
      routeAccountingPeriod={accountingPeriod ?? null}
    />
  );
};

export type { UpdateFundViewParams, UpdateFundViewSearchParams };
export default UpdateFundView;
