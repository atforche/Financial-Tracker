import AccountingPeriodFundFrame from "@/accounting-periods/funds/AccountingPeriodFundFrame";
import type { FundTransactionSortOrder } from "@/funds/types";
import { GoalFrameContext } from "@/goals/GoalFrame";
import type { JSX } from "react";
import breadcrumbs from "@/accounting-periods/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the AccountingPeriodFundView component.
 */
interface AccountingPeriodFundViewParams {
  id: string;
  fundId: string;
}

/**
 * Search parameters for the AccountingPeriodFundView component.
 */
interface AccountingPeriodFundViewSearchParams {
  search?: string;
  sort?: FundTransactionSortOrder;
}

/**
 * Props for the AccountingPeriodFundView component.
 */
interface AccountingPeriodFundViewProps {
  readonly params: Promise<AccountingPeriodFundViewParams>;
  readonly searchParams: Promise<AccountingPeriodFundViewSearchParams>;
}

/**
 * Component that displays the view for a single fund in the context of an accounting period.
 */
const AccountingPeriodFundView = async function ({
  params,
  searchParams,
}: AccountingPeriodFundViewProps): Promise<JSX.Element> {
  const { id, fundId } = await params;
  const { search, sort } = await searchParams;

  const apiClient = getApiClient();
  const accountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId: id,
        },
      },
    },
  );
  const fundPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/funds/{fundId}",
    {
      params: {
        path: {
          fundId,
          accountingPeriodId: id,
        },
      },
    },
  );
  const goalPromise = apiClient.GET("/goals", {
    params: {
      query: {
        AccountingPeriodId: id,
        fundId,
      },
    },
  });
  const fundTransactionsPromise = apiClient.GET(
    "/funds/{fundId}/transactions",
    {
      params: {
        path: {
          fundId,
        },
        query: {
          AccountingPeriodId: id,
          Search: search ?? "",
          Sort: sort ?? null,
        },
      },
    },
  );
  const [
    { data: accountingPeriod, error: accountingPeriodError },
    { data: fund, error: fundError },
    { data: goal },
    { data: transactions, error: fundTransactionsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    goalPromise,
    fundTransactionsPromise,
  ]);

  if (
    typeof accountingPeriod === "undefined" ||
    typeof fund === "undefined" ||
    typeof transactions === "undefined"
  ) {
    throw new Error(
      `Failed to fetch fund with ID ${fundId} for accounting period with ID ${id}: ${accountingPeriodError?.detail ?? fundError?.detail ?? fundTransactionsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <AccountingPeriodFundFrame
      breadcrumbs={breadcrumbs.fundDetail(accountingPeriod, fund)}
      accountingPeriod={accountingPeriod}
      fund={fund}
      goal={goal ?? null}
      transactions={transactions.items}
      transactionsTotalCount={transactions.totalCount}
      context={GoalFrameContext.FundDetail}
    />
  );
};

export type {
  AccountingPeriodFundViewParams,
  AccountingPeriodFundViewSearchParams,
};
export default AccountingPeriodFundView;
