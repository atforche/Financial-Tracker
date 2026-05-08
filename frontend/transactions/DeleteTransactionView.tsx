import DeleteTransactionForm from "@/transactions/DeleteTransactionForm";
import type { JSX } from "react";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the delete transaction view component.
 */
interface DeleteTransactionViewParams {
  readonly id: string;
}

/**
 * Search parameters for the delete transaction view component.
 */
interface DeleteTransactionViewSearchParams {
  readonly accountingPeriodId?: string | null;
  readonly accountId?: string | null;
  readonly fundId?: string | null;
}

/**
 * Props for the DeleteTransactionView component.
 */
interface DeleteTransactionViewProps {
  readonly params: Promise<DeleteTransactionViewParams>;
  readonly searchParams: Promise<DeleteTransactionViewSearchParams>;
}

/**
 * Component that displays the delete transaction view.
 */
const DeleteTransactionView = async function ({
  params,
  searchParams,
}: DeleteTransactionViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId, accountId, fundId } = await searchParams;

  const apiClient = getApiClient();
  const transactionPromise = apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId: id,
      },
    },
  });
  const accountingPeriodPromise =
    accountingPeriodId !== null && typeof accountingPeriodId !== "undefined"
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: {
            path: {
              accountingPeriodId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const accountPromise =
    accountId !== null && typeof accountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const fundPromise =
    fundId !== null && typeof fundId !== "undefined"
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const [
    { data: transaction },
    { data: accountingPeriod },
    { data: account },
    { data: fund },
  ] = await Promise.all([
    transactionPromise,
    accountingPeriodPromise,
    accountPromise,
    fundPromise,
  ]);
  if (typeof transaction === "undefined") {
    throw new Error("Failed to fetch transaction");
  }

  return (
    <DeleteTransactionForm
      transaction={transaction}
      routeAccountingPeriod={accountingPeriod ?? null}
      routeAccount={account ?? null}
      routeFund={fund ?? null}
    />
  );
};

export type { DeleteTransactionViewParams, DeleteTransactionViewSearchParams };
export default DeleteTransactionView;
