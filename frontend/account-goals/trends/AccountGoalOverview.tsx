import type {
  AccountGoalPeriodProgress,
  AccountGoalTrendPoint,
} from "@/account-goals/trends/accountGoalProgressTrends";
import { Box, Stack, Typography } from "@mui/material";
import AccountGoalCompletionGauge from "@/account-goals/trends/AccountGoalCompletionGauge";
import { AccountGoalEndingBalanceStatus } from "@/account-goals/types";
import type { AccountGoalTrendsSearchParams } from "@/account-goals/trends/helpers";
import type { AccountingPeriod } from "@/accounting-periods/types";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import Link from "next/link";
import routes from "@/account-goals/routes";

interface Props {
  readonly points: readonly AccountGoalTrendPoint[];
  readonly periods: readonly AccountingPeriod[];
  readonly progressByPeriod: ReadonlyMap<
    string,
    readonly AccountGoalPeriodProgress[]
  >;
  readonly accounts: readonly { readonly id: string; readonly name: string }[];
  readonly searchParams: AccountGoalTrendsSearchParams;
}

/** Summarizes completion by period and exposes the account-level results behind it. */
const AccountGoalOverview = function ({
  points,
  periods,
  progressByPeriod,
  accounts,
  searchParams,
}: Props): JSX.Element {
  const goalCount = points.reduce(
    (sum, point) => sum + point.configuredGoalCount,
    0,
  );
  const achievedCount = points.reduce(
    (sum, point) => sum + point.satisfiedGoalCount,
    0,
  );
  const completionPercentage =
    goalCount === 0 ? null : (achievedCount / goalCount) * 100;
  const orderedPoints = [...points].reverse();
  const orderedPeriods = [...periods].reverse();
  const byPeriodAndAccount = new Map<string, AccountGoalPeriodProgress>();
  progressByPeriod.forEach((entries, periodId) => {
    entries.forEach((entry) => {
      byPeriodAndAccount.set(
        `${periodId}:${entry.accountGoal.account.id}`,
        entry,
      );
    });
  });

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
            <AccountGoalCompletionGauge percentage={completionPercentage} />
          </Box>
          <Box sx={{ minWidth: 0 }}>
            <Stack spacing={2.25}>
              {orderedPoints.map((point) => {
                const total = point.configuredGoalCount;
                return (
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
                      role="img"
                      aria-label={`${point.accountingPeriodName}: ${total === 0 ? "no goals" : `${point.satisfiedGoalCount} of ${total} goals met; ${point.belowMinimumCount} below minimum, ${point.withinRangeCount} within range, ${point.aboveMaximumCount} above maximum`}`}
                    >
                      {total > 0 ? (
                        <>
                          <Box
                            sx={{
                              width: `${(point.belowMinimumCount / total) * 100}%`,
                              bgcolor: "warning.main",
                            }}
                          />
                          <Box
                            sx={{
                              width: `${(point.withinRangeCount / total) * 100}%`,
                              bgcolor: "success.main",
                            }}
                          />
                          <Box
                            sx={{
                              width: `${(point.aboveMaximumCount / total) * 100}%`,
                              bgcolor: "info.main",
                            }}
                          />
                        </>
                      ) : null}
                    </Box>
                    <Typography
                      variant="body2"
                      fontWeight={700}
                      sx={{ width: { sm: 120 }, textAlign: { sm: "right" } }}
                    >
                      {total === 0
                        ? "No goals"
                        : `${point.satisfiedGoalCount}/${total} · ${Math.round(point.satisfiedPercentage)}%`}
                    </Typography>
                  </Stack>
                );
              })}
            </Stack>
            <Stack direction="row" gap={2} flexWrap="wrap" sx={{ mt: 2.5 }}>
              <Typography variant="caption" color="warning.main">
                ■ Below minimum
              </Typography>
              <Typography variant="caption" color="success.main">
                ■ Within range
              </Typography>
              <Typography variant="caption" color="info.main">
                ■ Above maximum
              </Typography>
            </Stack>
          </Box>
        </Box>
      </Frame>

      <Frame title="Account Results">
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
            {accounts.map((account) => (
              <Box key={account.id} sx={{ display: "contents" }}>
                <Box
                  sx={{
                    py: 0.5,
                    pr: 1,
                    "& a:hover": { color: "primary.main" },
                    "& a:focus-visible": {
                      outline: "2px solid",
                      outlineColor: "primary.main",
                      outlineOffset: 3,
                      borderRadius: 0.5,
                    },
                  }}
                >
                  <Link
                    style={{
                      color: "inherit",
                      textDecoration: "underline",
                      fontWeight: 600,
                    }}
                    href={routes.trends({
                      ...searchParams,
                      accountId: account.id,
                    })}
                  >
                    {account.name}
                  </Link>
                </Box>
                {orderedPeriods.map((period) => {
                  const entry = byPeriodAndAccount.get(
                    `${period.id}:${account.id}`,
                  );
                  const status = entry?.progress.endingBalance.status;
                  const label =
                    entry === undefined
                      ? "No goal"
                      : period.isOpen
                        ? entry.progress.isSatisfied
                          ? "On track so far"
                          : status ===
                              AccountGoalEndingBalanceStatus.BelowMinimum
                            ? "Below minimum so far"
                            : "Above maximum so far"
                        : entry.progress.isSatisfied
                          ? "Met"
                          : status ===
                              AccountGoalEndingBalanceStatus.BelowMinimum
                            ? "Below minimum"
                            : "Above maximum";
                  const color =
                    status === AccountGoalEndingBalanceStatus.WithinRange
                      ? "success.main"
                      : status === AccountGoalEndingBalanceStatus.BelowMinimum
                        ? "warning.main"
                        : status === AccountGoalEndingBalanceStatus.AboveMaximum
                          ? "info.main"
                          : "action.disabledBackground";
                  const symbol =
                    status === AccountGoalEndingBalanceStatus.WithinRange
                      ? "✓"
                      : status === AccountGoalEndingBalanceStatus.BelowMinimum
                        ? "↓"
                        : status === AccountGoalEndingBalanceStatus.AboveMaximum
                          ? "↑"
                          : "—";
                  return (
                    <Box
                      key={period.id}
                      role="img"
                      aria-label={`${account.name}, ${period.name}: ${label}`}
                      title={`${account.name}, ${period.name}: ${label}`}
                      sx={{
                        height: 32,
                        borderRadius: 1,
                        bgcolor: color,
                        color:
                          status === AccountGoalEndingBalanceStatus.WithinRange
                            ? "common.white"
                            : "common.black",
                        fontWeight: 700,
                        textAlign: "center",
                        lineHeight: "32px",
                        opacity: entry === undefined ? 0.4 : 1,
                      }}
                    >
                      {symbol}
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

export default AccountGoalOverview;
