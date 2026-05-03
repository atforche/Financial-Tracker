import type { JSX } from "react";
import PostTransactionForm from "@/transactions/PostTransactionForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the post transaction view component.
 */
interface PostTransactionViewParams {
  readonly id: string;
}

/**
 * Search parameters for the post transaction view component.
 */
interface PostTransactionViewSearchParams {
  readonly accountingPeriodId?: string | null;
  readonly accountId?: string | null;
}

/**
 * Props for the PostTransactionView component.
 */
interface PostTransactionViewProps {
  readonly params: Promise<PostTransactionViewParams>;
  readonly searchParams: Promise<PostTransactionViewSearchParams>;
}

/**
 * Component that displays the post transaction view.
 */
const PostTransactionView = async function ({
  params,
  searchParams,
}: PostTransactionViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId, accountId } = await searchParams;

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
  const [{ data: transaction }, { data: accountingPeriod }, { data: account }] =
    await Promise.all([
      transactionPromise,
      accountingPeriodPromise,
      accountPromise,
    ]);
  if (typeof transaction === "undefined") {
    throw new Error(`Failed to fetch transaction with ID ${id}"}`);
  }
  return (
    <PostTransactionForm
      transaction={transaction}
      providedAccountingPeriod={accountingPeriod ?? null}
      providedAccount={account ?? null}
    />
  );
};

export type { PostTransactionViewParams, PostTransactionViewSearchParams };
export default PostTransactionView;
