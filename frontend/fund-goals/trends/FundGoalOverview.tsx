import { Box, Stack, Typography } from "@mui/material";
import {
  type FundGoalPeriodProgress,
  type FundGoalTrendPoint,
  isFundGoalSatisfied,
} from "@/fund-goals/trends/fundGoalProgressTrends";
import AccountGoalCompletionGauge from "@/account-goals/trends/AccountGoalCompletionGauge";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Frame from "@/framework/view/Frame";
import type { FundGoalTrendsSearchParams } from "@/fund-goals/trends/helpers";
import type { JSX } from "react";
import Link from "next/link";
import routes from "@/fund-goals/routes";

interface Props {
  readonly points: readonly FundGoalTrendPoint[];
  readonly periods: readonly AccountingPeriod[];
  readonly progressByPeriod: ReadonlyMap<
    string,
    readonly FundGoalPeriodProgress[]
  >;
  readonly funds: readonly { id: string; name: string }[];
  readonly searchParams: FundGoalTrendsSearchParams;
}

/** Overview of Fund Goal completion and each fund's period results. */
const FundGoalOverview = function ({
  points,
  periods,
  progressByPeriod,
  funds,
  searchParams,
}: Props): JSX.Element {
  const baseParams = { ...searchParams };
  delete baseParams.fundName;
  const count = points.reduce(
    (total, point) => total + point.configuredGoalCount,
    0,
  );
  const achieved = points.reduce(
    (total, point) => total + point.satisfiedGoalCount,
    0,
  );
  const byPeriodAndFund = new Map<string, FundGoalPeriodProgress>();
  progressByPeriod.forEach((entries, periodId) => {
    entries.forEach((entry) => {
      byPeriodAndFund.set(`${periodId}:${entry.fundGoal.fund.id}`, entry);
    });
  });
  const orderedPeriods = [...periods].reverse();
  return (
    <Stack spacing={3}>
      <Frame title="Goal Completion">
        <Box
          sx={{
            p: { xs: 1, md: 1.5 },
            display: "grid",
            gridTemplateColumns: { xs: "1fr", md: "200px minmax(0, 1fr)" },
            gap: 3,
          }}
        >
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              pb: { xs: 3, md: 0 },
              pr: { md: 3 },
              borderBottom: { xs: "1px solid", md: "none" },
              borderRight: { md: "1px solid" },
              borderColor: "divider",
            }}
          >
            <AccountGoalCompletionGauge
              percentage={count === 0 ? null : (achieved / count) * 100}
            />
          </Box>
          <Box sx={{ minWidth: 0 }}>
            <Stack spacing={2.25}>
              {[...points].reverse().map((point) => (
                <Stack
                  key={point.accountingPeriodId}
                  direction={{ xs: "column", sm: "row" }}
                  spacing={1.5}
                  alignItems={{ sm: "center" }}
                >
                  <Box sx={{ width: { sm: 125 }, flexShrink: 0 }}>
                    <Typography fontWeight={700} variant="body2">
                      {point.accountingPeriodName}
                    </Typography>
                    {point.isOpen ? (
                      <Typography variant="caption" color="text.secondary">
                        Open · so far
                      </Typography>
                    ) : null}
                  </Box>
                  <Box
                    role="img"
                    aria-label={`${point.accountingPeriodName}: ${point.satisfiedGoalCount} of ${point.configuredGoalCount} goals met`}
                    sx={{
                      flex: { xs: "none", sm: 1 },
                      width: { xs: "100%", sm: "auto" },
                      minWidth: 0,
                      display: "flex",
                      height: 22,
                      overflow: "hidden",
                      borderRadius: 2,
                      bgcolor: "action.hover",
                    }}
                  >
                    {point.configuredGoalCount > 0 && (
                      <>
                        <Box
                          sx={{
                            width: `${point.satisfiedPercentage}%`,
                            bgcolor: "success.main",
                          }}
                        />
                        <Box
                          sx={{
                            width: `${100 - point.satisfiedPercentage}%`,
                            bgcolor: "error.main",
                          }}
                        />
                      </>
                    )}
                  </Box>
                  <Typography
                    variant="body2"
                    fontWeight={700}
                    sx={{ width: { sm: 120 }, textAlign: { sm: "right" } }}
                  >
                    {point.configuredGoalCount === 0
                      ? "No goals"
                      : `${point.satisfiedGoalCount}/${point.configuredGoalCount} · ${Math.round(point.satisfiedPercentage)}%`}
                  </Typography>
                </Stack>
              ))}
            </Stack>
            <Stack direction="row" gap={2} flexWrap="wrap" sx={{ mt: 2.5 }}>
              <Typography variant="caption" color="success.main">
                ■ Met
              </Typography>
              <Typography variant="caption" color="error.main">
                ■ Not met
              </Typography>
            </Stack>
          </Box>
        </Box>
      </Frame>
      <Frame title="Fund Results">
        <Box sx={{ p: { xs: 1, md: 1.5 }, overflowX: "auto" }}>
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: `minmax(150px, 1.5fr) repeat(${orderedPeriods.length}, minmax(86px, 1fr))`,
              minWidth: Math.max(400, 150 + orderedPeriods.length * 86),
              gap: 0.75,
              alignItems: "center",
            }}
          >
            <Box />
            {orderedPeriods.map((period) => (
              <Typography
                key={period.id}
                variant="caption"
                fontWeight={700}
                textAlign="center"
              >
                {period.name}
              </Typography>
            ))}
            {funds.map((fund) => (
              <Box key={fund.id} sx={{ display: "contents" }}>
                <Box
                  sx={{
                    py: 0.5,
                    pr: 1,
                    "& a:hover": { color: "primary.main" },
                  }}
                >
                  <Link
                    style={{
                      color: "inherit",
                      textDecoration: "underline",
                      fontWeight: 600,
                    }}
                    href={routes.trends({ ...baseParams, fundId: fund.id })}
                  >
                    {fund.name}
                  </Link>
                </Box>
                {orderedPeriods.map((period) => {
                  const entry = byPeriodAndFund.get(`${period.id}:${fund.id}`);
                  const met = entry ? isFundGoalSatisfied(entry) : false;
                  const label = !entry
                    ? "No goal"
                    : met
                      ? "Met"
                      : "Not met";
                  const color = !entry
                    ? "action.disabledBackground"
                    : met
                      ? "success.main"
                      : "error.main";
                  return (
                    <Box
                      key={period.id}
                      role="img"
                      aria-label={`${fund.name}, ${period.name}: ${label}`}
                      title={`${fund.name}, ${period.name}: ${label}`}
                      sx={{
                        height: 32,
                        borderRadius: 1,
                        bgcolor: color,
                        color: "common.white",
                        fontWeight: 700,
                        textAlign: "center",
                        lineHeight: "32px",
                        opacity: entry ? 1 : 0.4,
                      }}
                    >
                      {!entry ? null : met ? "✓" : "!"}
                    </Box>
                  );
                })}
              </Box>
            ))}
          </Box>
        </Box>
      </Frame>
    </Stack>
  );
};

export default FundGoalOverview;
