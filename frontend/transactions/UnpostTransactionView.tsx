import type { JSX } from "react";
import UnpostTransactionForm from "@/transactions/UnpostTransactionForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the unpost transaction view component.
 */
interface UnpostTransactionViewParams {
  readonly id: string;
}

/**
 * Search parameters for the unpost transaction view component.
 */
interface UnpostTransactionViewSearchParams {
  readonly accountingPeriodId?: string | null;
  readonly accountId?: string | null;
}

/**
 * Props for the UnpostTransactionView component.
 */
interface UnpostTransactionViewProps {
  readonly params: Promise<UnpostTransactionViewParams>;
  readonly searchParams: Promise<UnpostTransactionViewSearchParams>;
}

/**
 * Component that displays the unpost transaction view.
 */
const UnpostTransactionView = async function ({
  params,
  searchParams,
}: UnpostTransactionViewProps): Promise<JSX.Element> {
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
    throw new Error(`Failed to fetch transaction with ID ${id}`);
  }

  return (
    <UnpostTransactionForm
      transaction={transaction}
      routeAccountingPeriod={accountingPeriod ?? null}
      routeAccount={account ?? null}
    />
  );
};

export type { UnpostTransactionViewParams, UnpostTransactionViewSearchParams };
export default UnpostTransactionView;
