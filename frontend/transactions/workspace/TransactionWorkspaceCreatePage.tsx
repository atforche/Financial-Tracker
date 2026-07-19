import CreateTransactionForm from "@/transactions/workspace/CreateTransactionForm";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import { getTransactionWorkspaceReferenceData } from "@/transactions/workspace/getTransactionWorkspaceData";
import routes from "@/transactions/routes";

/**
 * Props for the TransactionWorkspaceCreatePage component.
 */
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
    <PageLayout>
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
    </PageLayout>
  );
};

export default TransactionWorkspaceCreatePage;
