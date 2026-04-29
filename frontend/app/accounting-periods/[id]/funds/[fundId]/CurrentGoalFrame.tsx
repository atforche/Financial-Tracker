import { Button, Stack, Typography } from "@mui/material";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { FundGoal } from "@/data/fundTypes";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/framework/routes";

/**
 * Props for the CurrentGoalFrame component.
 */
interface CurrentGoalFrameProps {
  readonly fundId: string;
  readonly fundGoal: FundGoal | null;
  readonly accountingPeriodId: string;
  readonly isAccountingPeriodOpen: boolean;
  readonly isSystemFund: boolean;
}

/**
 * Component that displays the current goal for a fund and accounting period.
 */
const CurrentGoalFrame = function ({
  fundId,
  fundGoal,
  accountingPeriodId,
  isAccountingPeriodOpen,
  isSystemFund,
}: CurrentGoalFrameProps): JSX.Element {
  return (
    <CaptionedFrame caption="Current Goal" maxWidth={null}>
      <Stack
        direction="row"
        spacing={2}
        justifyContent="space-between"
        alignItems="center"
        sx={{ width: 1000 }}
      >
        {fundGoal !== null ? (
          <Stack width={500}>
            <CaptionedValue
              caption="Goal Type"
              value={fundGoal.goalType ?? "-"}
            />
            <CaptionedValue
              caption="Goal Amount"
              value={formatCurrency(fundGoal.goalAmount)}
            />
          </Stack>
        ) : (
          <Typography variant="subtitle1">No current goal.</Typography>
        )}
        {fundGoal !== null ? (
          <Stack direction="row" spacing={1}>
            <Button
              variant="contained"
              color="primary"
              href={routes.accountingPeriods.fundGoalUpdate(
                accountingPeriodId,
                fundGoal.fundId,
              )}
              disabled={isSystemFund || !isAccountingPeriodOpen}
            >
              Edit
            </Button>
            <Button
              variant="contained"
              color="error"
              href={routes.accountingPeriods.fundGoalDelete(
                accountingPeriodId,
                fundGoal.fundId,
              )}
              disabled={isSystemFund || !isAccountingPeriodOpen}
            >
              Delete
            </Button>
          </Stack>
        ) : (
          <Button
            variant="contained"
            color="primary"
            href={routes.accountingPeriods.fundGoalCreate(
              accountingPeriodId,
              fundId,
            )}
            disabled={isSystemFund || !isAccountingPeriodOpen}
          >
            Add
          </Button>
        )}
      </Stack>
      {fundGoal !== null && (
        <>
          <CaptionedValue
            caption="Remaining Amount to Assign"
            value={formatCurrency(fundGoal.remainingAmountToAssign)}
          />
          <CaptionedValue
            caption="Is Assignment Goal Met"
            value={fundGoal.isAssignmentGoalMet ? "Yes" : "No"}
          />
          <CaptionedValue
            caption="Remaining Amount to Spend"
            value={formatCurrency(fundGoal.remainingAmountToSpend)}
          />
          <CaptionedValue
            caption="Is Spending Goal Met"
            value={fundGoal.isSpendingGoalMet ? "Yes" : "No"}
          />
        </>
      )}
    </CaptionedFrame>
  );
};

export default CurrentGoalFrame;
