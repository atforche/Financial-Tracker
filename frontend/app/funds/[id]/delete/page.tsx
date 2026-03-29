import DeleteFundForm from "@/app/funds/[id]/delete/DeleteFundForm";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
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
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
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

export default Page;
