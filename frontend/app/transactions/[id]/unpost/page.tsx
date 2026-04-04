import type { JSX } from "react";
import UnpostTransactionForm from "@/app/transactions/[id]/unpost/UnpostTransactionForm";
import type { UnpostTransactionFormSearchParams } from "@/app/transactions/[id]/unpost/unpostTransactionFormSearchParams";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{ id: string }>;
  readonly searchParams: Promise<UnpostTransactionFormSearchParams>;
}

/**
 * Component that displays the unpost transaction view.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
  const { id } = await params;
  const { accountId, accountingPeriodId, fundId } = await searchParams;
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
      `Failed to load data for unpost transaction page: ${transactionError?.detail ?? accountingPeriodError?.detail ?? accountError?.detail ?? fundError?.detail ?? "Unknown error"}`,
    );
  }

  const redirectParams = new URLSearchParams();
  if (typeof accountingPeriodId !== "undefined") {
    redirectParams.set("accountingPeriodId", accountingPeriodId);
  } else if (typeof fundId !== "undefined") {
    redirectParams.set("fundId", fundId);
  } else {
    redirectParams.set("accountId", accountId);
  }
  const redirectQueryString = redirectParams.toString();
  const redirectUrl = `/transactions/${id}${redirectQueryString ? `?${redirectQueryString}` : ""}`;

  return (
    <UnpostTransactionForm
      transaction={transactionData}
      redirectUrl={redirectUrl}
      providedAccountingPeriod={accountingPeriodData}
      providedAccount={accountData}
      providedFund={fundData}
    />
  );
};

export default Page;
