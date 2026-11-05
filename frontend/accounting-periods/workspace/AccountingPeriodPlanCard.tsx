"use client";

import { Divider, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import {
  compareCurrencyAmounts,
  formatCurrency,
  formatSignedCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import BreakdownSection from "@/framework/view/BreakdownSection";
import ContentSurface from "@/framework/view/ContentSurface";
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
 * A fund's planned contribution that its maximum ending balance is projected to leave unused.
 */
interface UnusedContribution {
  readonly id: string;
  readonly name: string;
  readonly amount: number;
  readonly href: Route;
}

/**
 * Props for the AccountingPeriodPlanCard component.
 */
interface AccountingPeriodPlanCardProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly unusedContributions: readonly UnusedContribution[];
}

/**
 * Summarizes the income an accounting period expects and how much of it its goals plan to take, noting any planned
 * contributions that funds at their maximum ending balances are projected not to use.
 */
const AccountingPeriodPlanCard = function ({
  accountingPeriod,
  unusedContributions,
}: AccountingPeriodPlanCardProps): JSX.Element {
  const { expectedIncome } = accountingPeriod;
  const [expanded, setExpanded] = useState(false);
  const totalUnused = getCurrencyDifference(
    accountingPeriod.plannedGoalContributions,
    accountingPeriod.expectedGoalContributions,
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
        {compareCurrencyAmounts(totalUnused, 0) === 0 ? null : (
          <>
            <Divider />
            <BreakdownSection
              label="Projected unused contributions"
              value={formatSignedCurrency(totalUnused)}
              detailRows={unusedContributions.map((fund) => ({
                key: fund.id,
                label: fund.name,
                value: formatCurrency(fund.amount),
                href: fund.href,
              }))}
              expanded={expanded}
              onToggle={() => {
                setExpanded(!expanded);
              }}
            />
          </>
        )}
      </Stack>
    </ContentSurface>
  );
};

export default AccountingPeriodPlanCard;
