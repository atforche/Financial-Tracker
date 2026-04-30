import { Button, Stack, Typography } from "@mui/material";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { Goal } from "@/data/goalTypes";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/framework/routes";

/**
 * Props for the CurrentGoalFrame component.
 */
interface CurrentGoalFrameProps {
  readonly fundId: string;
  readonly goal: Goal | null;
  readonly accountingPeriodId: string;
  readonly isAccountingPeriodOpen: boolean;
  readonly isSystemFund: boolean;
}

/**
 * Component that displays the current goal for a fund and accounting period.
 */
const CurrentGoalFrame = function ({
  fundId,
  goal,
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
        {goal !== null ? (
          <Stack width={500}>
            <CaptionedValue caption="Goal Type" value={goal.goalType} />
            <CaptionedValue
              caption="Goal Amount"
              value={formatCurrency(goal.goalAmount)}
            />
          </Stack>
        ) : (
          <Typography variant="subtitle1">No current goal.</Typography>
        )}
        {goal !== null ? (
          <Stack direction="row" spacing={1}>
            <Button
              variant="contained"
              color="primary"
              href={routes.accountingPeriods.goalUpdate(
                accountingPeriodId,
                goal.fundId,
              )}
              disabled={isSystemFund || !isAccountingPeriodOpen}
            >
              Edit
            </Button>
            <Button
              variant="contained"
              color="error"
              href={routes.accountingPeriods.goalDelete(
                accountingPeriodId,
                goal.fundId,
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
            href={routes.accountingPeriods.goalCreate(
              accountingPeriodId,
              fundId,
            )}
            disabled={isSystemFund || !isAccountingPeriodOpen}
          >
            Add
          </Button>
        )}
      </Stack>
      {goal !== null && (
        <>
          <CaptionedValue
            caption="Remaining Amount to Assign"
            value={formatCurrency(goal.remainingAmountToAssign)}
          />
          <CaptionedValue
            caption="Is Assignment Goal Met"
            value={goal.isAssignmentGoalMet ? "Yes" : "No"}
          />
          <CaptionedValue
            caption="Remaining Amount to Spend"
            value={formatCurrency(goal.remainingAmountToSpend)}
          />
          <CaptionedValue
            caption="Is Spending Goal Met"
            value={goal.isSpendingGoalMet ? "Yes" : "No"}
          />
        </>
      )}
    </CaptionedFrame>
  );
};

export default CurrentGoalFrame;
