import type { JSX } from "react";
import UpdateAccountForm from "@/app/accounts/[id]/update/UpdateAccountForm";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page component.
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
 * Component that displays the update account view.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId } = await searchParams;

  const apiClient = getApiClient();

  const accountPromise = apiClient.GET("/accounts/{accountId}", {
    params: {
      path: {
        accountId: id,
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

  const [{ data: accountData }, { data: accountingPeriodData }] =
    await Promise.all([accountPromise, accountingPeriodPromise]);

  if (typeof accountData === "undefined") {
    throw new Error("Failed to fetch account data");
  }

  return (
    <UpdateAccountForm
      account={accountData}
      providedAccountingPeriod={accountingPeriodData ?? null}
    />
  );
};

export default Page;
