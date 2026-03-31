import type { JSX } from "react";
import UpdateTransactionForm from "@/app/transactions/[id]/update/UpdateTransactionForm";
import getApiClient from "@/data/getApiClient";

/**
 * Props for the Page Component.
 */
interface PageProps {
  readonly params: Promise<{
    id: string;
  }>;
  readonly searchParams: Promise<{
    accountingPeriodId?: string;
    accountId?: string;
    fundId?: string;
  }>;
}

/**
 * Component that displays the update transaction view.
 */
const Page = async function ({
  params,
  searchParams,
}: PageProps): Promise<JSX.Element> {
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
  const fundsPromise = apiClient.GET("/funds");
  const accountingPeriodPromise =
    typeof accountingPeriodId !== "undefined"
      ? apiClient.GET("/accounting-periods/{accountingPeriodId}", {
          params: {
            path: {
              accountingPeriodId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });
  const accountPromise =
    typeof accountId !== "undefined"
      ? apiClient.GET("/accounts/{accountId}", {
          params: {
            path: {
              accountId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });
  const fundPromise =
    typeof fundId !== "undefined"
      ? apiClient.GET("/funds/{fundId}", {
          params: {
            path: {
              fundId,
            },
          },
        })
      : Promise.resolve({ data: null, error: null });

  const [
    { data: transactionData, error: transactionError },
    { data: fundsData, error: fundsError },
    { data: accountingPeriodData, error: accountingPeriodError },
    { data: accountData, error: accountError },
    { data: fundData, error: fundError },
  ] = await Promise.all([
    transactionPromise,
    fundsPromise,
    accountingPeriodPromise,
    accountPromise,
    fundPromise,
  ]);

  if (
    typeof transactionData === "undefined" ||
    typeof fundsData === "undefined" ||
    typeof accountingPeriodData === "undefined" ||
    typeof accountData === "undefined" ||
    typeof fundData === "undefined"
  ) {
    throw new Error(
      `Failed to fetch data for update transaction page: ${transactionError?.detail ?? fundsError?.detail ?? accountingPeriodError?.detail ?? accountError?.detail ?? fundError?.detail ?? "Unknown error"}`,
    );
  }

  const transactionAccountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: transactionData.accountingPeriodId,
        },
      },
    },
  );
  const { data: transactionAccountingPeriodData } =
    await transactionAccountingPeriodPromise;

  if (typeof transactionAccountingPeriodData === "undefined") {
    throw new Error("Failed to fetch accounting period for transaction");
  }

  return (
    <UpdateTransactionForm
      transaction={transactionData}
      accountingPeriod={transactionAccountingPeriodData}
      funds={fundsData.items.map((fund) => ({ id: fund.id, name: fund.name }))}
      providedAccountingPeriod={accountingPeriodData}
      providedAccount={accountData}
      providedFund={fundData}
    />
  );
};

export default Page;
