import {
  getTransactionById,
  getTransactionWorkspaceDetailReferenceData,
} from "@/transactions/workspace/getTransactionWorkspaceData";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import ViewTransactionForm from "@/transactions/workspace/ViewTransactionForm";
import getTransactionPageBackLink from "@/transactions/workspace/getTransactionPageBackLink";
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
  const back = getTransactionPageBackLink(returnUrl, workspaceUrl);

  if (transaction === null) {
    redirect(back.href);
  }

  const referenceData = await getTransactionWorkspaceDetailReferenceData(
    transaction.accountingPeriodId,
  );

  return (
    <PageLayout>
      <TransactionWorkspacePageHeader
        backHref={back.href}
        backLabel={back.label}
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
        editUrl={routes.workspaceEdit(
          transaction.id,
          selectedWorkspaceSearchParams,
        )}
        deleteRedirectUrl={back.href}
      />
    </PageLayout>
  );
};

export default TransactionWorkspaceDetailPage;
