import {
  getTransactionById,
  getTransactionWorkspaceReferenceData,
} from "@/transactions/workspace/getTransactionWorkspaceData";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import TransactionWorkspacePageHeader from "@/transactions/workspace/TransactionWorkspacePageHeader";
import type { TransactionWorkspaceSearchParams } from "@/transactions/workspace/TransactionWorkspace";
import ViewTransactionForm from "@/transactions/workspace/ViewTransactionForm";
import { redirect } from "next/navigation";
import routes from "@/transactions/routes";

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
  const { accountingPeriodIds, accountIds, fundIds, sort, page } =
    resolvedSearchParams;
  const [referenceData, transaction] = await Promise.all([
    getTransactionWorkspaceReferenceData(),
    getTransactionById(transactionId),
  ]);

  const workspaceSearchParams: TransactionWorkspaceSearchParams = {
    ...(typeof accountingPeriodIds !== "undefined"
      ? { accountingPeriodIds }
      : {}),
    ...(typeof accountIds !== "undefined" ? { accountIds } : {}),
    ...(typeof fundIds !== "undefined" ? { fundIds } : {}),
    ...(typeof sort !== "undefined" ? { sort } : {}),
    ...(typeof page !== "undefined" ? { page } : {}),
    selectedTransactionId: transactionId,
  };
  const workspaceUrl = routes.workspace(workspaceSearchParams);

  if (transaction === null) {
    redirect(
      routes.workspace({
        ...(typeof accountingPeriodIds !== "undefined"
          ? { accountingPeriodIds }
          : {}),
        ...(typeof accountIds !== "undefined" ? { accountIds } : {}),
        ...(typeof fundIds !== "undefined" ? { fundIds } : {}),
        ...(typeof sort !== "undefined" ? { sort } : {}),
        ...(typeof page !== "undefined" ? { page } : {}),
      }),
    );
  }

  const transactionAccountingPeriod =
    referenceData.allAccountingPeriods.find(
      (period) => period.id === transaction.accountingPeriodId,
    ) ?? null;

  if (transactionAccountingPeriod === null) {
    throw new Error("Failed to find the transaction accounting period");
  }

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <TransactionWorkspacePageHeader
        backHref={workspaceUrl}
        title="Transaction Details"
      />
      <ViewTransactionForm
        transaction={transaction}
        transactionAccountingPeriod={transactionAccountingPeriod}
        funds={referenceData.funds}
        assignmentGoals={referenceData.assignmentGoals}
        spendingGoals={referenceData.spendingGoals}
        currentUrl={routes.workspaceDetail(
          transaction.id,
          workspaceSearchParams,
        )}
        workspaceUrl={workspaceUrl}
        editUrl={routes.workspaceEdit(transaction.id, workspaceSearchParams)}
      />
    </Stack>
  );
};

export const dynamic = "force-dynamic";
export default TransactionWorkspaceDetailPage;
