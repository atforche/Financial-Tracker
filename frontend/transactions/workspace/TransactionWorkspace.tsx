import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import type { TransactionSort } from "@/transactions/types";
import TransactionWorkspaceFilter from "@/transactions/workspace/TransactionWorkspaceFilter";
import TransactionWorkspaceListFrame from "@/transactions/workspace/TransactionWorkspaceListFrame";
import { getTransactionWorkspaceListData } from "@/transactions/workspace/getTransactionWorkspaceData";

/**
 * Search parameters supported by the Transactions workspace.
 */
interface TransactionWorkspaceSearchParams {
  accountingPeriodIds?: string | string[];
  accountIds?: string | string[];
  fundIds?: string | string[];
  sort?: TransactionSort | null;
  page?: number | string | null;
  pageSize?: number | string | null;
  selectedTransactionId?: string;
  returnUrl?: string;
}

/**
 * Props for the TransactionWorkspace component.
 */
interface TransactionWorkspaceProps {
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

/**
 * Renders the main transaction workspace, including filters and the transaction list.
 */
const TransactionWorkspace = async function ({
  searchParams,
}: TransactionWorkspaceProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { openAccountingPeriods, accounts, funds, transactions } =
    await getTransactionWorkspaceListData(resolvedSearchParams);

  return (
    <ConstrainedContent>
      <PageLayout>
        <TransactionWorkspaceFilter
          accountingPeriods={openAccountingPeriods}
          accounts={accounts}
          funds={funds}
        />
        <TransactionWorkspaceListFrame
          data={transactions.items}
          totalCount={transactions.totalCount}
        />
      </PageLayout>
    </ConstrainedContent>
  );
};

export type { TransactionWorkspaceSearchParams };
export default TransactionWorkspace;
