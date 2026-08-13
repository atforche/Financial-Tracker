import ExpectedIncomeSourceForm, {
  type ExpectedIncomeSourceMode,
} from "@/accounting-periods/workspace/ExpectedIncomeSourceForm";
import type { ExpectedIncomeSource } from "@/accounting-periods/types";
import ExpectedIncomeSourceDetailsDialog from "@/accounting-periods/workspace/ExpectedIncomeSourceDetailsDialog";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the ExpectedIncomeSourcePage component.
 */
interface ExpectedIncomeSourcePageProps {
  readonly accountingPeriodId: string;
  readonly sourceId?: string | undefined;
  readonly mode: ExpectedIncomeSourceMode;
  readonly returnUrl: string | undefined;
}

/**
 * Displays an expected-income source in either view or edit mode, depending on the provided props.
 */
const ExpectedIncomeSourcePage = async function ({
  accountingPeriodId,
  sourceId,
  mode,
  returnUrl,
}: ExpectedIncomeSourcePageProps): Promise<JSX.Element> {
  const apiClient = await createApiClient();
  const response = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    {
      params: { path: { accountingPeriodId } },
    },
  );
  const accountingPeriod = unwrapApiResponse(
    response,
    "Failed to fetch accounting period",
  );
  const backHref = returnUrl ?? routes.workspaceDetail(accountingPeriodId, {});
  const source: ExpectedIncomeSource | undefined = isNotNullOrUndefined(
    sourceId,
  )
    ? accountingPeriod.expectedIncomeSources.find(
        (item) => item.id === sourceId,
      )
    : undefined;
  if (sourceId !== undefined && source === undefined) {
    redirect(backHref);
  }
  if (mode === "view" && source !== undefined) {
    return (
      <ExpectedIncomeSourceDetailsDialog
        source={source}
        accountingPeriodId={accountingPeriodId}
        existingSources={accountingPeriod.expectedIncomeSources}
        canManage={accountingPeriod.isOpen}
        backHref={backHref}
        editHref={routes.expectedIncomeSourceEdit(
          accountingPeriodId,
          source.id,
          backHref,
        )}
      />
    );
  }
  return (
    <ExpectedIncomeSourceForm
      accountingPeriod={accountingPeriod}
      mode={mode}
      {...(source === undefined ? {} : { source })}
      backHref={backHref}
      redirectUrl={backHref}
    />
  );
};

export default ExpectedIncomeSourcePage;
