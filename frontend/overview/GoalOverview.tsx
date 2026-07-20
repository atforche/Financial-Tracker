import { Stack, Typography } from "@mui/material";
import type { AccountingPeriod } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import createApiClient from "@/framework/data/createApiClient";
import { formatCurrency } from "@/framework/currencyHelpers";
import loadAllPages from "@/framework/data/loadAllPages";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Props for the GoalOverview component.
 */
interface GoalOverviewProps {
  readonly latestAccountingPeriod: AccountingPeriod | null;
}

/**
 * Overview component for goals.
 */
const GoalOverview = async function ({
  latestAccountingPeriod,
}: GoalOverviewProps): Promise<JSX.Element> {
  if (latestAccountingPeriod === null) {
    return (
      <ContentSurface>
        <Stack spacing={2}>
          <Typography variant="caption" color="text.secondary">
            Current Goals
          </Typography>
          <Typography variant="h5">Goals</Typography>
          <Typography color="text.secondary">
            No current accounting period is available to show goal summaries.
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
      "Failed to load goals",
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
          Current Goals ({latestAccountingPeriod.name})
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
export default GoalOverview;
