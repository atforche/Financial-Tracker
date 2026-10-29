import { Divider, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  formatSignedCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";

/**
 * Props for the PlanRow component.
 */
interface PlanRowProps {
  readonly label: string;
  readonly value: JSX.Element | string;
  readonly isTotal?: boolean;
}

/**
 * Displays one labeled figure of the plan.
 */
const PlanRow = function ({
  label,
  value,
  isTotal = false,
}: PlanRowProps): JSX.Element {
  return (
    <Stack
      direction="row"
      spacing={2}
      sx={{
        alignItems: "baseline",
        justifyContent: "space-between",
      }}
    >
      <Typography variant={isTotal ? "subtitle2" : "body2"}>{label}</Typography>
      <Typography variant="body2" sx={{ fontWeight: 500 }}>
        {value}
      </Typography>
    </Stack>
  );
};

/**
 * Props for the AccountingPeriodPlanCard component.
 */
interface AccountingPeriodPlanCardProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly cappedGoalCount: number;
  readonly fundGoalsHref: Route;
}

/**
 * Summarizes the income an accounting period expects and how much of it its goals plan to take, including how much
 * maximum ending balances reduce what the goals are expected to take.
 */
const AccountingPeriodPlanCard = function ({
  accountingPeriod,
  cappedGoalCount,
  fundGoalsHref,
}: AccountingPeriodPlanCardProps): JSX.Element {
  const { expectedIncome } = accountingPeriod;
  const maximumReduction = getCurrencyDifference(
    accountingPeriod.expectedGoalContributions,
    accountingPeriod.plannedGoalContributions,
  );
  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Typography variant="h5">Plan</Typography>
        <PlanRow
          label="Expected income"
          value={formatCurrency(expectedIncome.total)}
        />
        <PlanRow
          label="Expected tracked income"
          value={formatCurrency(expectedIncome.tracked)}
        />
        <Divider />
        <PlanRow
          label="Planned goal contributions"
          value={formatCurrency(accountingPeriod.plannedGoalContributions)}
        />
        <PlanRow
          label="Planned left to spend"
          value={
            <SignedChange
              amount={getCurrencyDifference(
                expectedIncome.tracked,
                accountingPeriod.plannedGoalContributions,
              )}
            />
          }
          isTotal
        />
        <Divider />
        {compareCurrencyAmounts(maximumReduction, 0) === 0 ? null : (
          <Stack spacing={0.25}>
            <PlanRow
              label="Reduced by maximum balances"
              value={formatSignedCurrency(maximumReduction)}
            />
            <Typography variant="caption" color="text.secondary">
              <Typography component="span" variant="inherit" color="primary">
                <Link
                  href={fundGoalsHref}
                  style={{ color: "inherit", textDecoration: "none" }}
                >
                  {cappedGoalCount === 1
                    ? "1 goal is"
                    : `${cappedGoalCount.toString()} goals are`}
                </Link>
              </Typography>{" "}
              near their maximum balance
            </Typography>
          </Stack>
        )}
        <PlanRow
          label="Expected goal contributions"
          value={formatCurrency(accountingPeriod.expectedGoalContributions)}
        />
        <PlanRow
          label="Expected left to spend"
          value={
            <SignedChange
              amount={getCurrencyDifference(
                expectedIncome.tracked,
                accountingPeriod.expectedGoalContributions,
              )}
            />
          }
          isTotal
        />
      </Stack>
    </ContentSurface>
  );
};

export default AccountingPeriodPlanCard;
