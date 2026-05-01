import type { JSX } from "react";
import UpdateAccountForm from "@/accounts/UpdateAccountForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the UpdateAccountView component.
 */
interface UpdateAccountViewProps {
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
const UpdateAccountView = async function ({
  params,
  searchParams,
}: UpdateAccountViewProps): Promise<JSX.Element> {
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

export default UpdateAccountView;
