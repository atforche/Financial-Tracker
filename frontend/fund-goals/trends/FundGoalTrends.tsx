import { Stack, Typography } from "@mui/material";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import createApiClient from "@/framework/data/createApiClient";
import { formatCurrency } from "@/framework/currencyHelpers";
import { isNullOrUndefined } from "@/framework/nullHelpers";
import loadAllPages from "@/framework/data/loadAllPages";
import { redirect } from "next/navigation";
import routes from "@/fund-goals/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters for the FundGoalTrends component.
 */
interface FundGoalTrendsSearchParams {
  fundName?: string | string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}
interface FundGoalTrendsProps {
  readonly searchParams: Promise<FundGoalTrendsSearchParams>;
}

/**
 * Displays Fund Goal trends.
 */
const FundGoalTrends = async function ({
  searchParams,
}: FundGoalTrendsProps): Promise<JSX.Element> {
  const params = await searchParams;
  const apiClient = createApiClient();
  const periods = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/accounting-periods", {
        params: {
          query: { Sort: AccountingPeriodSort.DateDescending, limit, offset },
        },
      }),
      "Failed to load accounting periods",
    ),
  );
  const latest = periods[0] ?? null;
  if (
    (isNullOrUndefined(params.startAccountingPeriodId) ||
      isNullOrUndefined(params.endAccountingPeriodId)) &&
    latest
  ) {
    redirect(
      routes.trends({
        startAccountingPeriodId: latest.id,
        endAccountingPeriodId: latest.id,
      }),
    );
  }
  const start = params.startAccountingPeriodId ?? latest?.id;
  const end = params.endAccountingPeriodId ?? latest?.id;
  const fundNames = toRepeatedSearchParams(params.fundName);
  const fundGoals = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/fund-goals", {
        params: {
          query: {
            "Filter.AccountingPeriodIds": periods
              .filter(
                (p) =>
                  (isNullOrUndefined(start) ||
                    p.id === start ||
                    periods.findIndex((x) => x.id === p.id) <=
                      periods.findIndex((x) => x.id === start)) &&
                  (isNullOrUndefined(end) ||
                    p.id === end ||
                    periods.findIndex((x) => x.id === p.id) >=
                      periods.findIndex((x) => x.id === end)),
              )
              .map((p) => p.id),
            limit,
            offset,
          },
        },
      }),
      "Failed to load Fund Goals",
    ),
  );
  const visible = fundGoals.filter(
    (fundGoal) =>
      fundNames.length === 0 || fundNames.includes(fundGoal.fund.name),
  );
  const configured = visible.filter(
    (fundGoal) =>
      fundGoal.regularContribution !== null ||
      fundGoal.minimumFundedBalance !== null ||
      fundGoal.maximumFundedBalance !== null ||
      fundGoal.targetEndingBalance !== null,
  );
  const totalContribution = visible.reduce(
    (sum, fundGoal) => sum + (fundGoal.regularContribution ?? 0),
    0,
  );
  return (
    <PageLayout>
      <ConstrainedContent>
        <Frame title="Goal Trends">
          <Stack spacing={2}>
            <Typography color="text.secondary">
              Review how Fund Goal configuration changes across accounting
              periods.
            </Typography>
            <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
              <Frame title="Goals">
                <Typography variant="h4">{visible.length}</Typography>
              </Frame>
              <Frame title="Configured Goals">
                <Typography variant="h4">{configured.length}</Typography>
              </Frame>
              <Frame title="Regular Contributions">
                <Typography variant="h4">
                  {formatCurrency(totalContribution)}
                </Typography>
              </Frame>
            </ResponsiveGrid>
          </Stack>
        </Frame>
      </ConstrainedContent>
      <ResponsiveGrid minimumColumnWidth={320} spacing={2}>
        {visible.map((fundGoal) => (
          <Frame key={fundGoal.id} title={fundGoal.fund.name}>
            <Stack spacing={0.75}>
              <Typography color="text.secondary">
                {fundGoal.accountingPeriod?.name ?? "Onboarded"}
              </Typography>
              <Typography>
                Regular contribution:{" "}
                {isNullOrUndefined(fundGoal.regularContribution)
                  ? "Not configured"
                  : formatCurrency(fundGoal.regularContribution)}
              </Typography>
              <Typography>
                Funded range:{" "}
                {isNullOrUndefined(fundGoal.minimumFundedBalance)
                  ? "No minimum"
                  : formatCurrency(fundGoal.minimumFundedBalance)}{" "}
                –{" "}
                {isNullOrUndefined(fundGoal.maximumFundedBalance)
                  ? "No maximum"
                  : formatCurrency(fundGoal.maximumFundedBalance)}
              </Typography>
              <Typography>
                Target ending balance:{" "}
                {isNullOrUndefined(fundGoal.targetEndingBalance)
                  ? "Not configured"
                  : formatCurrency(fundGoal.targetEndingBalance)}
              </Typography>
            </Stack>
          </Frame>
        ))}
      </ResponsiveGrid>
    </PageLayout>
  );
};
export type { FundGoalTrendsSearchParams };
export default FundGoalTrends;
