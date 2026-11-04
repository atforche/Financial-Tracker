"use client";

import type { FundGoal, FundGoalProgress } from "@/fund-goals/types";
import { type JSX, useState } from "react";
import { Button } from "@mui/material";
import Dialog from "@/framework/dialog/Dialog";
import FundGoalProgressOverview from "@/fund-goals/workspace/FundGoalProgressOverview";

/**
 * Opens one period's contribution and ending-balance breakdown.
 */
const FundGoalDetailsButton = function ({
  goal,
  progress,
}: {
  readonly goal: FundGoal;
  readonly progress: FundGoalProgress;
}): JSX.Element {
  const [open, setOpen] = useState(false);
  const close = (): void => {
    setOpen(false);
  };
  return (
    <>
      <Button
        onClick={() => {
          setOpen(true);
        }}
      >
        Details
      </Button>
      <Dialog
        open={open}
        fullWidth
        maxWidth="sm"
        title={`${goal.accountingPeriod?.name ?? "Fund Goal"} Details`}
        onClose={close}
        actions={<Button onClick={close}>Close</Button>}
      >
        <FundGoalProgressOverview fundGoal={goal} progress={progress} />
      </Dialog>
    </>
  );
};

export default FundGoalDetailsButton;
