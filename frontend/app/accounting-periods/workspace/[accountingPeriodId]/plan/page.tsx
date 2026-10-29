import type { AccountingPeriodSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspaceDetailPage";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";

/**
 * Sends links to the former plan page to the accounting period's page, which now includes its plan.
 */
const AccountingPeriodPlanRedirect = async function ({
  params,
  searchParams,
}: {
  readonly params: Promise<{ accountingPeriodId: string }>;
  readonly searchParams: Promise<AccountingPeriodSearchParams>;
}): Promise<never> {
  redirect(
    routes.workspaceDetail(
      (await params).accountingPeriodId,
      await searchParams,
    ),
  );
};

export default AccountingPeriodPlanRedirect;
