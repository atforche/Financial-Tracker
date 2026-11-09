import AccountSetupForm from "@/accounts/workspace/AccountSetupForm";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import getFinancialInstitutions from "@/accounts/workspace/getFinancialInstitutions";
import { redirect } from "next/navigation";
import routes from "@/accounts/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Page for creating an account in an open period, or onboarding one before any periods exist. Each redirects to the
 * workspace when the other is the one that applies.
 */
const AccountWorkspaceSetupPage = async function ({
  mode,
}: {
  readonly mode: "create" | "onboard";
}): Promise<JSX.Element> {
  const workspaceUrl = routes.workspace({});
  const apiClient = await createApiClient();
  const [boundsResponse, openPeriodsResponse, financialInstitutions] =
    await Promise.all([
      apiClient.GET("/accounting-periods/bounds"),
      mode === "create"
        ? apiClient.GET("/accounting-periods", {
            params: { query: { "Filter.IsOpen": true } },
          })
        : null,
      getFinancialInstitutions(),
    ]);
  const isInOnboardingMode =
    unwrapApiResponse(
      boundsResponse,
      "Failed to fetch the accounting period bounds",
    ).latest === null;
  if (isInOnboardingMode !== (mode === "onboard")) {
    redirect(workspaceUrl);
  }

  return openPeriodsResponse === null ? (
    <AccountSetupForm
      mode="onboard"
      financialInstitutions={financialInstitutions}
      redirectUrl={workspaceUrl}
    />
  ) : (
    <AccountSetupForm
      mode="create"
      accountingPeriods={
        unwrapApiResponse(
          openPeriodsResponse,
          "Failed to fetch accounting periods",
        ).items
      }
      financialInstitutions={financialInstitutions}
      redirectUrl={workspaceUrl}
    />
  );
};

export default AccountWorkspaceSetupPage;
