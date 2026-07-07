import CreateTransactionForm from "@/transactions/workspace/CreateTransactionForm";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { getTransactionWorkspaceReferenceData } from "@/transactions/workspace/getTransactionWorkspaceData";
import routes from "@/transactions/routes";

interface TransactionWorkspaceCreatePageProps {
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

/**
 * Page for creating a new transaction within the workspace.
 */
const TransactionWorkspaceCreatePage = async function ({
  searchParams,
}: TransactionWorkspaceCreatePageProps): Promise<JSX.Element> {
  const resolvedSearchParams = await searchParams;
  const { accountingPeriodIds, accountIds, fundIds, sort, page, returnUrl } =
    resolvedSearchParams;
  const {
    openAccountingPeriods,
    accounts,
    funds,
    assignmentGoals,
    spendingGoals,
  } = await getTransactionWorkspaceReferenceData();

  const workspaceSearchParams: TransactionWorkspaceSearchParams = {
    ...(typeof accountingPeriodIds !== "undefined"
      ? { accountingPeriodIds }
      : {}),
    ...(typeof accountIds !== "undefined" ? { accountIds } : {}),
    ...(typeof fundIds !== "undefined" ? { fundIds } : {}),
    ...(typeof sort !== "undefined" ? { sort } : {}),
    ...(typeof page !== "undefined" ? { page } : {}),
    ...(typeof returnUrl !== "undefined" ? { returnUrl } : {}),
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionWorkspacePageHeader
        backHref={returnUrl ?? workspaceUrl}
        title="Create Transaction"
      />
      <CreateTransactionForm
        accountingPeriods={openAccountingPeriods}
        accounts={accounts}
        funds={funds}
        assignmentGoals={assignmentGoals}
        spendingGoals={spendingGoals}
        redirectUrl={workspaceUrl}
        showHeading={false}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default TransactionWorkspaceCreatePage;
