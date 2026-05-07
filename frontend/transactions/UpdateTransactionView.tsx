import type { JSX } from "react";
import UpdateTransactionForm from "@/transactions/UpdateTransactionForm";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the update transaction view component.
 */
interface UpdateTransactionViewParams {
  readonly id: string;
}

/**
 * Search parameters for the update transaction view component.
 */
interface UpdateTransactionViewSearchParams {
  readonly accountingPeriodId?: string | null;
  readonly accountId?: string | null;
  readonly fundId?: string | null;
}

/**
 * Props for the UpdateTransactionView component.
 */
interface UpdateTransactionViewProps {
  readonly params: Promise<UpdateTransactionViewParams>;
  readonly searchParams: Promise<UpdateTransactionViewSearchParams>;
}

/**
 * Component that displays the update transaction view.
 */
const UpdateTransactionView = async function ({
  params,
  searchParams,
}: UpdateTransactionViewProps): Promise<JSX.Element> {
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
  const fundsPromise = apiClient.GET("/funds");
  const [
    { data: transaction },
    { data: accountingPeriod },
    { data: account },
    { data: funds },
  ] = await Promise.all([
    transactionPromise,
    accountingPeriodPromise,
    accountPromise,
    fundsPromise,
  ]);
  if (typeof transaction === "undefined" || typeof funds === "undefined") {
    throw new Error(`Failed to fetch transaction with ID ${id}"}`);
  }
  const fund =
    typeof fundId !== "undefined" && fundId !== null
      ? (funds.items.find((f) => f.id === fundId) ?? null)
      : null;

  const transactionAccountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: transaction.accountingPeriodId,
        },
      },
    },
  );
  const transactionDebitAccountPromise =
    "debitAccount" in transaction && transaction.debitAccount !== null
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId: transaction.debitAccount.accountId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const transactionCreditAccountPromise =
    "creditAccount" in transaction && transaction.creditAccount !== null
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId: transaction.creditAccount.accountId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const transactionDebitFundPromise =
    "debitFund" in transaction
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId: transaction.debitFund.fundId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const transactionCreditFundPromise =
    "creditFund" in transaction
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId: transaction.creditFund.fundId,
            },
          },
        })
      : Promise.resolve({ data: null });
  const [
    { data: transactionAccountingPeriod },
    { data: transactionDebitAccount },
    { data: transactionCreditAccount },
    { data: transactionDebitFund },
    { data: transactionCreditFund },
  ] = await Promise.all([
    transactionAccountingPeriodPromise,
    transactionDebitAccountPromise,
    transactionCreditAccountPromise,
    transactionDebitFundPromise,
    transactionCreditFundPromise,
  ]);
  if (typeof transactionAccountingPeriod === "undefined") {
    throw new Error(
      `Failed to fetch accounting period with ID ${transaction.accountingPeriodId}"}`,
    );
  }

  return (
    <UpdateTransactionForm
      transaction={transaction}
      transactionAccountingPeriod={transactionAccountingPeriod}
      transactionDebitAccount={transactionDebitAccount ?? null}
      transactionCreditAccount={transactionCreditAccount ?? null}
      transactionDebitFund={transactionDebitFund ?? null}
      transactionCreditFund={transactionCreditFund ?? null}
      funds={funds.items}
      routeAccountingPeriod={accountingPeriod ?? null}
      routeAccount={account ?? null}
      routeFund={fund ?? null}
    />
  );
};

export type { UpdateTransactionViewParams, UpdateTransactionViewSearchParams };
export default UpdateTransactionView;
