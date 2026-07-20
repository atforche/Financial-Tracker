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
import routes from "@/fund-plans/routes";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters for the FundPlanTrends component.
 */
interface FundPlanTrendsSearchParams {
  fundName?: string | string[];
  startAccountingPeriodId?: string;
  endAccountingPeriodId?: string;
}
interface FundPlanTrendsProps {
  readonly searchParams: Promise<FundPlanTrendsSearchParams>;
}

/**
 * Displays Funding Plan trends.
 */
const FundPlanTrends = async function ({
  searchParams,
}: FundPlanTrendsProps): Promise<JSX.Element> {
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
  const plans = await loadAllPages(async (limit, offset) =>
    unwrapApiResponse(
      await apiClient.GET("/fund-plans", {
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
      "Failed to load Funding Plans",
    ),
  );
  const visible = plans.filter(
    (plan) => fundNames.length === 0 || fundNames.includes(plan.fund.name),
  );
  const configured = visible.filter(
    (plan) =>
      plan.regularContribution !== null ||
      plan.minimumFundedBalance !== null ||
      plan.maximumFundedBalance !== null ||
      plan.targetEndingBalance !== null,
  );
  const totalContribution = visible.reduce(
    (sum, plan) => sum + (plan.regularContribution ?? 0),
    0,
  );
  return (
    <PageLayout>
      <ConstrainedContent>
        <Frame title="Plan Trends">
          <Stack spacing={2}>
            <Typography color="text.secondary">
              Review how Funding Plan configuration changes across accounting
              periods.
            </Typography>
            <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
              <Frame title="Plans">
                <Typography variant="h4">{visible.length}</Typography>
              </Frame>
              <Frame title="Configured Plans">
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
        {visible.map((plan) => (
          <Frame key={plan.id} title={plan.fund.name}>
            <Stack spacing={0.75}>
              <Typography color="text.secondary">
                {plan.accountingPeriod?.name ?? "Onboarded"}
              </Typography>
              <Typography>
                Regular contribution:{" "}
                {isNullOrUndefined(plan.regularContribution)
                  ? "Not configured"
                  : formatCurrency(plan.regularContribution)}
              </Typography>
              <Typography>
                Funded range:{" "}
                {isNullOrUndefined(plan.minimumFundedBalance)
                  ? "No minimum"
                  : formatCurrency(plan.minimumFundedBalance)}{" "}
                –{" "}
                {isNullOrUndefined(plan.maximumFundedBalance)
                  ? "No maximum"
                  : formatCurrency(plan.maximumFundedBalance)}
              </Typography>
              <Typography>
                Target ending balance:{" "}
                {isNullOrUndefined(plan.targetEndingBalance)
                  ? "Not configured"
                  : formatCurrency(plan.targetEndingBalance)}
              </Typography>
            </Stack>
          </Frame>
        ))}
      </ResponsiveGrid>
    </PageLayout>
  );
};
export type { FundPlanTrendsSearchParams };
export default FundPlanTrends;
