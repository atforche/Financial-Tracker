import AccountingPeriodFundFrame from "@/accounting-periods/funds/AccountingPeriodFundFrame";
import type { FundTransactionSortOrder } from "@/funds/types";
import { GoalFrameContext } from "@/goals/GoalFrame";
import type { JSX } from "react";
import breadcrumbs from "@/goals/breadcrumbs";
import getApiClient from "@/framework/data/getApiClient";

/**
 * Parameters for the GoalDetailView component.
 */
interface GoalDetailViewParams {
  id: string;
}

/**
 * Search parameters for the GoalDetailView component.
 */
interface GoalDetailViewSearchParams {
  search?: string;
  sort?: FundTransactionSortOrder;
}

/**
 * Props for the GoalDetailView component.
 */
interface GoalDetailViewProps {
  readonly params: Promise<GoalDetailViewParams>;
  readonly searchParams: Promise<GoalDetailViewSearchParams>;
}

/**
 * Component that displays the view for a single goal.
 */
const GoalDetailView = async function ({
  params,
  searchParams,
}: GoalDetailViewProps): Promise<JSX.Element> {
  const { id } = await params;
  const { search, sort } = await searchParams;

  const apiClient = getApiClient();
  const { data: goal, error: goalError } = await apiClient.GET(
    "/goals/{goalId}",
    {
      params: {
        path: {
          goalId: id,
        },
      },
    },
  );

  if (typeof goal === "undefined") {
    throw new Error(
      `Failed to fetch goal with ID ${id}: ${goalError.detail ?? "Unknown error"}`,
    );
  }

  const { accountingPeriodId, fundId } = goal;
  const accountingPeriodPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: {
        path: {
          accountingPeriodId,
        },
      },
    },
  );
  const fundPromise = apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/funds/{fundId}",
    {
      params: {
        path: {
          accountingPeriodId,
          fundId,
        },
      },
    },
  );
  const fundTransactionsPromise = apiClient.GET(
    "/funds/{fundId}/transactions",
    {
      params: {
        path: {
          fundId,
        },
        query: {
          AccountingPeriodId: accountingPeriodId,
          Search: search ?? "",
          Sort: sort ?? null,
        },
      },
    },
  );

  const [
    { data: accountingPeriod, error: accountingPeriodError },
    { data: fund, error: fundError },
    { data: transactions, error: transactionsError },
  ] = await Promise.all([
    accountingPeriodPromise,
    fundPromise,
    fundTransactionsPromise,
  ]);

  if (
    typeof accountingPeriod === "undefined" ||
    typeof fund === "undefined" ||
    typeof transactions === "undefined"
  ) {
    throw new Error(
      `Failed to fetch goal view data: ${accountingPeriodError?.detail ?? fundError?.detail ?? transactionsError?.detail ?? "Unknown error"}`,
    );
  }

  return (
    <AccountingPeriodFundFrame
      breadcrumbs={breadcrumbs.detail(accountingPeriod, goal)}
      accountingPeriod={accountingPeriod}
      fund={fund}
      goal={goal}
      transactions={transactions.items}
      transactionsTotalCount={transactions.totalCount}
      context={GoalFrameContext.GoalDetail}
    />
  );
};

export type { GoalDetailViewParams, GoalDetailViewSearchParams };
export default GoalDetailView;
