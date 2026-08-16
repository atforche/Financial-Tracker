import {
  getTransactionById,
  getTransactionWorkspaceDetailReferenceData,
} from "@/transactions/workspace/getTransactionWorkspaceData";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import ViewTransactionForm from "@/transactions/workspace/ViewTransactionForm";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Props for the TransactionWorkspaceDetailPage component.
 */
interface TransactionWorkspaceDetailPageProps {
  readonly params: Promise<{
    transactionId: string;
  }>;
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

/**
 * Page for viewing details of a single transaction within the workspace.
 */
const TransactionWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: TransactionWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { transactionId } = await params;
  const resolvedSearchParams = await searchParams;
  const workspaceSearchParams = { ...resolvedSearchParams };
  delete workspaceSearchParams.selectedTransactionId;
  const { returnUrl } = workspaceSearchParams;
  const transaction = await getTransactionById(transactionId);

  const selectedWorkspaceSearchParams: TransactionWorkspaceSearchParams = {
    ...workspaceSearchParams,
    selectedTransactionId: transactionId,
  };
  const workspaceUrl = routes.workspace(selectedWorkspaceSearchParams);

  if (transaction === null) {
    redirect(routes.workspace(workspaceSearchParams));
  }

  const referenceData = await getTransactionWorkspaceDetailReferenceData(
    transaction.accountingPeriodId,
  );

  return (
    <PageLayout>
      <TransactionWorkspacePageHeader
        backHref={returnUrl ?? workspaceUrl}
        title="Transaction Details"
      />
      <ViewTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={referenceData.accountingPeriod}
        funds={referenceData.funds}
        fundGoals={referenceData.fundGoals}
        currentUrl={routes.workspaceDetail(
          transaction.id,
          selectedWorkspaceSearchParams,
        )}
        workspaceUrl={workspaceUrl}
        editUrl={routes.workspaceEdit(
          transaction.id,
          selectedWorkspaceSearchParams,
        )}
        returnUrl={returnUrl ?? null}
      />
    </PageLayout>
  );
};

export default TransactionWorkspaceDetailPage;
