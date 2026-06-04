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
 * Props for the DeleteTransactionView component.
 */
interface DeleteTransactionViewProps {
  readonly params: Promise<DeleteTransactionViewParams>;
}

/**
 * Component that displays the delete transaction view.
 */
const DeleteTransactionView = async function ({
  params,
}: DeleteTransactionViewProps): Promise<JSX.Element> {
  const { id } = await params;

  const apiClient = getApiClient();
  const transactionPromise = apiClient.GET("/transactions/{transactionId}", {
    params: {
      path: {
        transactionId: id,
      },
    },
  });
  const { data: transaction } = await transactionPromise;
  if (typeof transaction === "undefined") {
    throw new Error("Failed to fetch transaction");
  }

  return <DeleteTransactionForm transaction={transaction} />;
};

export type { DeleteTransactionViewParams };
export default DeleteTransactionView;
