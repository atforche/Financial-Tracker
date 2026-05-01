import DeleteAccountForm from "@/accounts/DeleteAccountForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Props for the DeleteAccountView component.
 */
interface DeleteAccountViewProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    accountingPeriodId?: string | null;
  }>;
}

/**
 * Component that displays the delete account view.
 */
const DeleteAccountView = async function ({
  params,
  searchParams,
}: DeleteAccountViewProps): Promise<JSX.Element> {
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
      : Promise.resolve({ data: null, error: null });

  const [{ data: account, error: accountError }, { data: accountingPeriod }] =
    await Promise.all([accountPromise, accountingPeriodPromise]);

  if (typeof account === "undefined") {
    throw new Error(`Failed to fetch account data: ${accountError.detail}`);
  }

  return (
    <DeleteAccountForm
      account={account}
      accountingPeriod={accountingPeriod ?? null}
    />
  );
};

export default DeleteAccountView;
