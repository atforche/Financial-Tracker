import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { formatCurrency } from "@/framework/currencyHelpers";
import loadAllPages from "@/framework/data/loadAllPages";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the FundPlanOverview component.
 */
interface FundPlanOverviewProps {
  readonly latestAccountingPeriod: AccountingPeriod | null;
}

/**
 * Overview component for Funding Plans.
 */
const FundPlanOverview = async function ({
  latestAccountingPeriod,
}: FundPlanOverviewProps): Promise<JSX.Element> {
  if (latestAccountingPeriod === null) {
    return (
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="caption" color="text.secondary">
            Current Plans
          </Typography>
          <Typography variant="h5">Plans</Typography>
          <Typography color="text.secondary">
            No current accounting period is available to show plan summaries.
          </Typography>
        </Stack>
      </ContentSurface>
    );
  }
  const apiClient = createApiClient();
  const plans = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/fund-plans", {
        params: {
          query: {
            "Filter.AccountingPeriodIds": [latestAccountingPeriod.id],
            Limit: limit,
            Offset: offset,
          },
        },
      }),
      "Failed to load Funding Plans",
    ),
  );
  const configured = plans.filter(
    (plan) =>
      plan.regularContribution !== null ||
      plan.minimumFundedBalance !== null ||
      plan.maximumFundedBalance !== null ||
      plan.targetEndingBalance !== null,
  );
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h6" color="text.secondary">
          Current Plans ({latestAccountingPeriod.name})
        </Typography>
        <Typography variant="h4">{configured.length} configured</Typography>
        <Typography color="text.secondary">
          Across {plans.length} funds with{" "}
          {formatCurrency(
            plans.reduce(
              (sum, plan) => sum + (plan.regularContribution ?? 0),
              0,
            ),
          )}{" "}
          in regular monthly contributions.
        </Typography>
      </Stack>
    </ContentSurface>
  );
};
export default FundPlanOverview;
