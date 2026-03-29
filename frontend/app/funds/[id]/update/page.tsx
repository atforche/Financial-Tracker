import type { JSX } from "react";
import UpdateFundForm from "@/app/funds/[id]/update/UpdateFundForm";
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
 * Component that displays the form for updating a fund.
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
      : Promise.resolve({ data: null });

  const [{ data: fundData }, { data: accountingPeriodData }] =
    await Promise.all([fundPromise, accountingPeriodPromise]);

  if (typeof fundData === "undefined") {
    throw new Error("Failed to fetch fund data");
  }

  return (
    <UpdateFundForm
      fund={fundData}
      providedAccountingPeriod={accountingPeriodData ?? null}
    />
  );
};

export default Page;
