import type { JSX } from "react";
import UpdateAccountForm from "@/accounts/UpdateAccountForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the UpdateAccountView component.
 */
interface UpdateAccountViewParams {
  id: string;
}

/**
 * Search parameters for the UpdateAccountView component.
 */
interface UpdateAccountViewSearchParams {
  accountingPeriodId?: string | null;
}

/**
 * Props for the UpdateAccountView component.
 */
interface UpdateAccountViewProps {
  readonly params: Promise<UpdateAccountViewParams>;
  readonly searchParams: Promise<UpdateAccountViewSearchParams>;
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

  const [{ data: account }, { data: accountingPeriod }] = await Promise.all([
    accountPromise,
    accountingPeriodPromise,
  ]);

  if (typeof account === "undefined") {
    throw new Error("Failed to fetch account data");
  }

  return (
    <UpdateAccountForm
      account={account}
      routeAccountingPeriod={accountingPeriod ?? null}
    />
  );
};

export type { UpdateAccountViewParams, UpdateAccountViewSearchParams };
export default UpdateAccountView;
