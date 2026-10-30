import ExpectedIncomeSourceDetails from "@/accounting-periods/workspace/ExpectedIncomeSourceDetails";
import ExpectedIncomeSourceForm from "@/accounting-periods/workspace/ExpectedIncomeSourceForm";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Whether an expected income source page views a source, or adds or changes one with the form.
 */
type ExpectedIncomeSourceMode = "view" | "add" | "change";

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
  const backHref = returnUrl ?? routes.workspaceDetail(accountingPeriodId, {});
  if (sourceId === undefined) {
    const response = await apiClient.GET(
      "/accounting-periods/expected-income-sources",
      {
        params: { query: { accountingPeriodIds: [accountingPeriodId] } },
      },
    );
    const [accountingPeriod] = unwrapApiResponse(
      response,
      "Failed to fetch accounting period",
    );
    if (accountingPeriod === undefined) {
      redirect(backHref);
    }
    return (
      <ExpectedIncomeSourceForm
        accountingPeriod={accountingPeriod}
        backHref={backHref}
        redirectUrl={backHref}
      />
    );
  }
  const response = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}/expected-income-sources/{expectedIncomeSourceId}",
    {
      params: {
        path: { accountingPeriodId, expectedIncomeSourceId: sourceId },
      },
    },
  );
  if (response.response.status === 404) {
    redirect(backHref);
  }
  const { accountingPeriod, source, income } = unwrapApiResponse(
    response,
    "Failed to fetch expected income source",
  );
  if (mode === "view") {
    const currentUrl = routes.expectedIncomeSource(
      accountingPeriodId,
      source.id,
      backHref,
    );
    return (
      <ExpectedIncomeSourceDetails
        source={source}
        income={income}
        accountingPeriodId={accountingPeriodId}
        accountingPeriodName={accountingPeriod.name}
        canManage={accountingPeriod.isOpen}
        backHref={backHref}
        currentUrl={currentUrl}
        editHref={routes.expectedIncomeSourceEdit(
          accountingPeriodId,
          source.id,
          currentUrl,
        )}
      />
    );
  }
  return (
    <ExpectedIncomeSourceForm
      accountingPeriod={accountingPeriod}
      source={source}
      backHref={backHref}
      redirectUrl={backHref}
    />
  );
};

export default ExpectedIncomeSourcePage;
