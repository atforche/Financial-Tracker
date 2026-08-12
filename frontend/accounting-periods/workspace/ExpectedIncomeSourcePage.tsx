import { Button, Stack, Typography } from "@mui/material";
import ExpectedIncomeSourceForm, {
  type ExpectedIncomeSourceMode,
} from "@/accounting-periods/workspace/ExpectedIncomeSourceForm";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import { ArrowBack } from "@mui/icons-material";
import ExpectedIncomeSourceDetails from "@/accounting-periods/workspace/ExpectedIncomeSourceDetails";
import type { JSX } from "react";
import Link from "next/link";
import PageLayout from "@/framework/view/PageLayout";
import createApiClient from "@/framework/data/createApiClient";
import { redirect } from "next/navigation";
import routes from "@/accounting-periods/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the ExpectedIncomeSourcePage component, including route parameters and search parameters.
 */
interface ExpectedIncomeSourcePageRouteProps {
  readonly params: Promise<{ accountingPeriodId: string; sourceId?: string }>;
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Props for the ExpectedIncomeSourcePage component, including route parameters, search parameters, and the mode of the page (view, add, or change).
 */
interface ExpectedIncomeSourcePageProps extends ExpectedIncomeSourcePageRouteProps {
  readonly mode: "view" | ExpectedIncomeSourceMode;
}

/**
 * Displays a dedicated expected-income source view or action page.
 */
const ExpectedIncomeSourcePage = async function ({
  params,
  searchParams,
  mode,
}: ExpectedIncomeSourcePageProps): Promise<JSX.Element> {
  const { accountingPeriodId, sourceId } = await params;
  const resolvedSearchParams = await searchParams;
  const apiClient = await createApiClient();
  const response = await apiClient.GET(
    "/accounting-periods/{accountingPeriodId}",
    { params: { path: { accountingPeriodId } } },
  );
  const backUrl = routes.workspaceDetail(
    accountingPeriodId,
    resolvedSearchParams,
  );
  if (response.error) {
    redirect(routes.workspace(resolvedSearchParams));
  }
  const period = unwrapApiResponse(
    response,
    "Failed to fetch accounting period",
  );
  const source =
    sourceId === undefined
      ? undefined
      : period.expectedIncomeSources.find((item) => item.id === sourceId);
  if (mode !== "add" && source === undefined) {
    redirect(backUrl);
  }
  if (
    mode !== "view" &&
    (!period.isOpen || (mode !== "add" && source === undefined))
  ) {
    redirect(
      source === undefined
        ? backUrl
        : routes.expectedIncomeDetail(
            period.id,
            source.id,
            resolvedSearchParams,
          ),
    );
  }
  const title =
    mode === "view"
      ? (source?.name ?? "Expected Income")
      : `${mode === "add" ? "Add" : "Edit"} Expected Income Source`;
  return (
    <PageLayout>
      <Stack spacing={2.5} sx={{ maxWidth: 1600, width: "100%" }}>
        <Link
          href={backUrl}
          style={{ alignSelf: "flex-start", textDecoration: "none" }}
        >
          <Button component="span" startIcon={<ArrowBack />}>
            Back to Accounting Period
          </Button>
        </Link>
        <Typography variant="h4">{title}</Typography>
        {mode === "view" && source !== undefined ? (
          <ExpectedIncomeSourceDetails
            accountingPeriod={period}
            source={source}
            backUrl={backUrl}
            editUrl={routes.expectedIncomeEdit(
              period.id,
              source.id,
              resolvedSearchParams,
            )}
            periodIsOpen={period.isOpen}
          />
        ) : (
          <ExpectedIncomeSourceForm
            accountingPeriod={period}
            mode={mode === "view" ? "add" : mode}
            {...(source === undefined ? {} : { source })}
            redirectUrl={backUrl}
            cancelUrl={
              source === undefined
                ? backUrl
                : routes.expectedIncomeDetail(
                    period.id,
                    source.id,
                    resolvedSearchParams,
                  )
            }
          />
        )}
      </Stack>
    </PageLayout>
  );
};

export type { ExpectedIncomeSourcePageRouteProps };
export default ExpectedIncomeSourcePage;
