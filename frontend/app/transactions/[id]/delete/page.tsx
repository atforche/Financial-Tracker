import DeleteTransactionForm from "@/app/transactions/[id]/delete/DeleteTransactionForm";
import type { DeleteTransactionSearchParams } from "@/app/transactions/[id]/delete/deleteTransactionSearchParams";
import type { JSX } from "react";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<DeleteTransactionSearchParams>;
}

/**
 * Gets the URL to redirect to after the transaction is deleted.
 */
const getRedirectUrl = function (
  accountingPeriodId?: string,
  accountId?: string,
  fundId?: string,
): string {
  if (typeof accountingPeriodId !== "undefined") {
    if (typeof accountId !== "undefined") {
      return `/accounting-periods/${accountingPeriodId}/accounts/${accountId}`;
    } else if (typeof fundId !== "undefined") {
      return `/accounting-periods/${accountingPeriodId}/funds/${fundId}`;
    }
    return `/accounting-periods/${accountingPeriodId}`;
  } else if (typeof accountId !== "undefined") {
    return `/accounts/${accountId}`;
  } else if (typeof fundId !== "undefined") {
    return `/funds/${fundId}`;
  }
  return "/transactions";
};

/**
 * Component that displays the form for deleting a transaction.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountingPeriodId, accountId, fundId } = await searchParams;
  const apiClient = getApiClient();

  const transactionPromise = apiClient.GET("/transactions/{transactionId}", {
    params: { path: { transactionId: id } },
  });
  const accountingPeriodPromise =
    typeof accountingPeriodId !== "undefined"
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: { path: { accountingPeriodId } },
        })
      : Promise.resolve({ data: null, error: null });
  const accountPromise =
    typeof accountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: { path: { accountId } },
        })
      : Promise.resolve({ data: null, error: null });
  const fundPromise =
    typeof fundId !== "undefined"
      ? apiClient.GET("/funds/{fundId}", { params: { path: { fundId } } })
      : Promise.resolve({ data: null, error: null });

  const [
    { data: transactionData, error: transactionError },
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: accountData, error: accountError },
    { data: fundData, error: fundError },
  ] = await Promise.all([
    transactionPromise,
    accountingPeriodPromise,
    accountPromise,
    fundPromise,
  ]);

  if (
    typeof transactionData === "undefined" ||
    typeof accountingPeriodData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof fundData === "undefined"
  ) {
    throw new Error(
      `Failed to load data for delete transaction page: ${transactionError?.detail ?? accountingPeriodError?.detail ?? accountError?.detail ?? fundError?.detail ?? "Unknown error"}`,
    );
  }

  const redirectUrl = getRedirectUrl(accountingPeriodId, accountId, fundId);

  return (
    <DeleteTransactionForm
      transaction={transactionData}
      redirectUrl={redirectUrl}
      providedAccountingPeriod={accountingPeriodData}
      providedAccount={accountData}
      providedFund={fundData}
    />
  );
};

export default Page;
