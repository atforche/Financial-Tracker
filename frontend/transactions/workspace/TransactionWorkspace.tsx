import type { TransactionSort, TransactionType } from "@/transactions/types";
import type { AccountType } from "@/accounts/types";
import ArrowBack from "@mui/icons-material/ArrowBack";
import { Button } from "@mui/material";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import TransactionWorkspaceFilter from "@/transactions/workspace/TransactionWorkspaceFilter";
import TransactionWorkspaceListFrame from "@/transactions/workspace/TransactionWorkspaceListFrame";
import { getTransactionWorkspaceListData } from "@/transactions/workspace/getTransactionWorkspaceData";

/**
 * Search parameters supported by the Transactions workspace.
 */
interface TransactionWorkspaceSearchParams {
  accountingPeriodIds?: string | readonly string[];
  accountIds?: string | readonly string[];
  fundIds?: string | readonly string[];
  fundNames?: string | readonly string[];
  accountTypes?: AccountType | readonly AccountType[];
  accountNames?: string | readonly string[];
  transactionTypes?: TransactionType | readonly TransactionType[];
  startDate?: string;
  endDate?: string;
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
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
  const {
    allAccountingPeriods,
    accounts,
    funds,
    selectedAccountIds,
    selectedFundIds,
    transactions,
  } = await getTransactionWorkspaceListData(resolvedSearchParams);

  return (
    <ConstrainedContent>
      <PageLayout>
        {typeof resolvedSearchParams.returnUrl === "undefined" ? null : (
          <Link
            href={resolvedSearchParams.returnUrl}
            style={{ alignSelf: "flex-start", textDecoration: "none" }}
          >
            <Button component="span" startIcon={<ArrowBack />}>
              Back to Trends
            </Button>
          </Link>
        )}
        <TransactionWorkspaceFilter
          accountingPeriods={allAccountingPeriods}
          accounts={accounts}
          funds={funds}
          selectedAccountIds={selectedAccountIds}
          selectedFundIds={selectedFundIds}
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
