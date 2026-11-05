"use client";

import BreakdownSection, {
  breakdownValueInset,
} from "@/framework/view/BreakdownSection";
import { Divider, Stack, Typography } from "@mui/material";
import { type JSX, useState } from "react";
import {
  compareCurrencyAmounts,
  formatCurrency,
  formatSignedCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
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
  readonly valueInset?: number;
}

/**
 * Displays one labeled figure of the plan.
 */
const PlanRow = function ({
  label,
  value,
  isTotal = false,
  valueInset = 0,
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
      <Typography
        variant="body2"
        sx={{ fontWeight: 500, pr: `${valueInset.toString()}px` }}
      >
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
  const hasUnused = compareCurrencyAmounts(totalUnused, 0) !== 0;
  // The unused contributions row keeps a chevron beside its amount, so the other amounts line up with it
  const valueInset = hasUnused ? breakdownValueInset : 0;
  return (
    <ContentSurface>
      <Stack spacing={1.5}>
        <Typography variant="h5">Plan</Typography>
        <PlanRow
          valueInset={valueInset}
          label="Expected income"
          value={formatCurrency(expectedIncome.total)}
        />
        <PlanRow
          valueInset={valueInset}
          label="Expected tracked income"
          value={formatCurrency(expectedIncome.tracked)}
        />
        <Divider />
        <PlanRow
          valueInset={valueInset}
          label="Planned goal contributions"
          value={formatCurrency(accountingPeriod.plannedGoalContributions)}
        />
        <PlanRow
          valueInset={valueInset}
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
        {!hasUnused ? null : (
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
