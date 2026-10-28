import CreateTransactionForm from "@/transactions/workspace/CreateTransactionForm";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import getTransactionPageBackLink from "@/transactions/workspace/getTransactionPageBackLink";
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
  const workspaceSearchParams = { ...resolvedSearchParams };
  delete workspaceSearchParams.selectedTransactionId;
  const { returnUrl } = workspaceSearchParams;
  const { openAccountingPeriods, accounts, funds, fundGoals, locations } =
    await getTransactionWorkspaceReferenceData();

  const workspaceUrl = routes.workspace(workspaceSearchParams);
  const back = getTransactionPageBackLink(returnUrl, workspaceUrl);

  return (
    <PageLayout>
      <TransactionWorkspacePageHeader
        backHref={back.href}
        backLabel={back.label}
        title="Create Transaction"
      />
      <CreateTransactionForm
        accountingPeriods={openAccountingPeriods}
        accounts={accounts}
        funds={funds}
        fundGoals={fundGoals}
        locations={locations}
        redirectUrl={workspaceUrl}
      />
    </PageLayout>
  );
};

export default TransactionWorkspaceCreatePage;
