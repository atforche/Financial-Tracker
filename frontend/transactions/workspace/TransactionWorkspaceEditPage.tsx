import {
  getTransactionById,
  getTransactionWorkspaceReferenceData,
} from "@/transactions/workspace/getTransactionWorkspaceData";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import UpdateTransactionForm from "@/transactions/workspace/UpdateTransactionForm";
import getTransactionPageBackLink from "@/transactions/workspace/getTransactionPageBackLink";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

/**
 * Props for the TransactionWorkspaceEditPage component.
 */
interface TransactionWorkspaceEditPageProps {
  readonly params: Promise<{
    transactionId: string;
  }>;
  readonly searchParams: Promise<TransactionWorkspaceSearchParams>;
}

/**
 * Page for editing a single transaction within the workspace.
 */
const TransactionWorkspaceEditPage = async function ({
  params,
  searchParams,
}: TransactionWorkspaceEditPageProps): Promise<JSX.Element> {
  const { transactionId } = await params;
  const resolvedSearchParams = await searchParams;
  const workspaceSearchParams = { ...resolvedSearchParams };
  delete workspaceSearchParams.selectedTransactionId;
  const { returnUrl } = workspaceSearchParams;
  const [referenceData, transaction] = await Promise.all([
    getTransactionWorkspaceReferenceData(),
    getTransactionById(transactionId),
  ]);

  const selectedWorkspaceSearchParams: TransactionWorkspaceSearchParams = {
    ...workspaceSearchParams,
    selectedTransactionId: transactionId,
  };
  const workspaceUrl = routes.workspace(selectedWorkspaceSearchParams);
  const back = getTransactionPageBackLink(returnUrl, workspaceUrl);

  if (transaction === null) {
    redirect(back.href);
  }

  // Closed periods can't be changed, so their transactions can only be viewed
  const transactionAccountingPeriod = referenceData.openAccountingPeriods.find(
    (period) => period.id === transaction.accountingPeriodId,
  );
  if (transactionAccountingPeriod === undefined) {
    redirect(
      routes.workspaceDetail(transactionId, selectedWorkspaceSearchParams),
    );
  }

  return (
    <PageLayout>
      <TransactionWorkspacePageHeader
        backHref={back.href}
        backLabel={back.label}
        title="Edit Transaction"
      />
      <UpdateTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        accounts={referenceData.accounts}
        funds={referenceData.funds}
        fundGoals={referenceData.fundGoals}
        expectedIncomeSources={referenceData.expectedIncomeSources}
        locations={referenceData.locations}
        redirectUrl={routes.workspaceDetail(
          transaction.id,
          selectedWorkspaceSearchParams,
        )}
      />
    </PageLayout>
  );
};

export default TransactionWorkspaceEditPage;
