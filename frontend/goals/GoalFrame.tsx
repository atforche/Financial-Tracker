import { Button, Stack, Typography } from "@mui/material";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { Goal } from "@/goals/types";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/goals/routes";

/**
 * Enumeration representing the context in which the GoalFrame is being used, which can affect its behavior and appearance.
 */
enum GoalFrameContext {
  FundDetail = "FundDetail",
  GoalDetail = "GoalDetail",
}

/**
 * Props for the GoalFrame component.
 */
interface GoalFrameProps {
  readonly goal: Goal | null;
  readonly fundId: string;
  readonly accountingPeriodId: string;
  readonly isAccountingPeriodOpen: boolean;
  readonly isSystemFund: boolean;
  readonly context: GoalFrameContext;
}

/**
 * Component that displays the current goal for a fund and accounting period.
 */
const GoalFrame = function ({
  fundId,
  goal,
  accountingPeriodId,
  isAccountingPeriodOpen,
  isSystemFund,
  context,
}: GoalFrameProps): JSX.Element {
  return (
    <CaptionedFrame
      caption={
        context === GoalFrameContext.FundDetail ? "Current Goal" : "Details"
      }
      maxWidth={null}
    >
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
              href={routes.update(
                { id: goal.id },
                {
                  fundId:
                    context === GoalFrameContext.FundDetail ? fundId : null,
                },
              )}
              disabled={isSystemFund || !isAccountingPeriodOpen}
            >
              Edit
            </Button>
            <Button
              variant="contained"
              color="error"
              href={routes.delete(
                { id: goal.id },
                {
                  fundId:
                    context === GoalFrameContext.FundDetail ? fundId : null,
                },
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
            href={routes.create({
              accountingPeriodId,
              fundId: context === GoalFrameContext.FundDetail ? fundId : null,
            })}
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

export { GoalFrameContext };
export default GoalFrame;
