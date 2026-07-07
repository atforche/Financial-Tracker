import type { JSX } from "react";
import { Stack } from "@mui/material";
import type { TransactionSortOrder } from "@/transactions/transaction";
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
  sort?: TransactionSortOrder | null;
  page?: number | string | null;
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
    <Stack spacing={3} sx={{ width: "100%", maxWidth: 1440 }}>
      <TransactionWorkspaceFilter
        accountingPeriods={openAccountingPeriods}
        accounts={accounts}
        funds={funds}
      />
      <TransactionWorkspaceListFrame
        data={transactions.items}
        totalCount={transactions.totalCount}
      />
    </Stack>
  );
};

export type { TransactionWorkspaceSearchParams };
export default TransactionWorkspace;
