import { Checkbox, Chip, FormControlLabel, Stack } from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import { type JSX, useEffect } from "react";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
  getCurrencyTotal,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import GoalAmountOption from "@/framework/forms/GoalAmountOption";

/**
 * Props for the FundGoalSetupSection component.
 */
interface FundGoalSetupSectionProps {
  readonly color?: FrameColor;
  readonly showFrame?: boolean;
  readonly accountingPeriod: AccountingPeriodWithBalance | null;
  readonly originalPlannedMonthlyContribution?: number | null;
  readonly plannedMonthlyContribution: number | null;
  readonly setPlannedMonthlyContribution:
    ((value: number | null) => void) | null;
  readonly minimumEndingBalance: number;
  readonly setMinimumEndingBalance: ((value: number) => void) | null;
  readonly maximumEndingBalance: number | null;
  readonly setMaximumEndingBalance: ((value: number | null) => void) | null;
  readonly allowExpectedContributionAboveMaximum: boolean;
  readonly setAllowExpectedContributionAboveMaximum:
    ((value: boolean) => void) | null;
}

/**
 * Renders the configurable quantities in a Fund Goal.
 */
const FundGoalSetupSection = function ({
  color = "primary",
  showFrame = true,
  accountingPeriod,
  originalPlannedMonthlyContribution = 0,
  plannedMonthlyContribution,
  setPlannedMonthlyContribution,
  minimumEndingBalance,
  setMinimumEndingBalance,
  maximumEndingBalance,
  setMaximumEndingBalance,
  allowExpectedContributionAboveMaximum,
  setAllowExpectedContributionAboveMaximum,
}: FundGoalSetupSectionProps): JSX.Element {
  const expectedIncome = accountingPeriod?.expectedIncome.tracked ?? 0;
  const totalPlannedContributions = getCurrencyTotal([
    accountingPeriod?.plannedGoalContributions ?? 0,
    -(originalPlannedMonthlyContribution ?? 0),
    plannedMonthlyContribution ?? 0,
  ]);
  const remaining = getCurrencyDifference(
    expectedIncome,
    totalPlannedContributions,
  );
  useEffect(() => {
    if (
      maximumEndingBalance === null &&
      allowExpectedContributionAboveMaximum
    ) {
      setAllowExpectedContributionAboveMaximum?.(false);
    }
  }, [
    allowExpectedContributionAboveMaximum,
    maximumEndingBalance,
    setAllowExpectedContributionAboveMaximum,
  ]);

  const content = (
    <Stack spacing={2}>
      <CurrencyEntryField
        label="Minimum Ending Balance"
        value={minimumEndingBalance}
        setValue={
          setMinimumEndingBalance === null
            ? null
            : (value): void => {
                setMinimumEndingBalance(value ?? 0);
              }
        }
      />
      <GoalAmountOption
        label="Maximum Ending Balance"
        value={maximumEndingBalance}
        setValue={setMaximumEndingBalance}
        errorMessage={null}
        additionalControl={
          <FormControlLabel
            sx={{
              ml: { xs: 0, sm: -1.375 },
              alignSelf: { xs: "flex-start", sm: "center" },
            }}
            control={
              <Checkbox
                checked={allowExpectedContributionAboveMaximum}
                disabled={maximumEndingBalance === null}
                onChange={(event): void =>
                  setAllowExpectedContributionAboveMaximum?.(
                    event.target.checked,
                  )
                }
              />
            }
            label="Allow expected contribution to exceed maximum"
          />
        }
      />
      <GoalAmountOption
        label="Planned Monthly Contribution"
        value={plannedMonthlyContribution}
        setValue={setPlannedMonthlyContribution}
        errorMessage={null}
      />
      {accountingPeriod !== null && plannedMonthlyContribution !== null ? (
        <Stack direction="row" flexWrap="wrap" useFlexGap spacing={0.75}>
          <Chip
            variant="outlined"
            label={`Expected tracked income: ${formatCurrency(expectedIncome)}`}
          />
          <Chip
            variant="outlined"
            label={`Total planned contributions: ${formatCurrency(totalPlannedContributions)}`}
          />
          <Chip
            variant="outlined"
            label={`Remaining: ${formatCurrency(remaining)}`}
            color={
              compareCurrencyAmounts(remaining, 0) < 0 ? "error" : "success"
            }
          />
        </Stack>
      ) : null}
    </Stack>
  );
  return showFrame ? (
    <Frame title="Fund Goal Setup" color={color}>
      {content}
    </Frame>
  ) : (
    content
  );
};

export default FundGoalSetupSection;
