import { Chip, Stack } from "@mui/material";
import type { Fund, FundIdentifier } from "@/funds/types";
import {
  type FundAssignmentDraft,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getSpendingGoalRemainingAmount,
  getSuggestedAmount,
  sortFundsByRemainingAmount,
  updateUnassignedFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import type { FrameColor } from "@/framework/view/Frame";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { JSX } from "react";
import type { SpendingGoal } from "@/goals/types";
import formatCurrency from "@/framework/formatCurrency";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the SpendingFundAssignmentPlanner component.
 */
interface SpendingFundAssignmentPlannerProps {
  readonly funds: Fund[];
  readonly spendingGoals: SpendingGoal[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments: FundAssignmentDraft[];
  readonly frameColor?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays the fund assignment planner for spending transactions.
 */
const SpendingFundAssignmentPlanner = function ({
  funds,
  spendingGoals,
  totalAmountToAssign,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments,
  frameColor = "primary",
  readOnly = false,
}: SpendingFundAssignmentPlannerProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);

  const sortFunds = function (
    left: FundIdentifier,
    right: FundIdentifier,
  ): number {
    return sortFundsByRemainingAmount(left, right, (fundId: string) =>
      getSpendingGoalRemainingAmount(
        fundId,
        spendingGoals,
        baselineFundAssignments,
      ),
    );
  };

  const addFundAssignment = function (): void {
    const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
    const nextFundAssignments: FundAssignmentDraft[] = [
      ...explicitFundAssignments,
      {
        fundId: "",
        fundName: "",
        amount: getSuggestedAmount(
          totalAmountToAssign,
          explicitFundAssignments,
          explicitFundAssignments.length,
        ),
        previousFundBalance: 0,
        newFundBalance: 0,
        previousGoalBalance: {
          remainingAmountToAssignIncludingPending: 0,
          remainingAmountToSpendIncludingPending: 0,
        },
        newGoalBalance: {
          remainingAmountToAssignIncludingPending: 0,
          remainingAmountToSpendIncludingPending: 0,
        },
      },
    ];
    setFundAssignments?.(
      updateUnassignedFundAmount(
        unassignedFund,
        totalAmountToAssign,
        nextFundAssignments,
      ),
    );
  };

  const deleteFundAssignment = function (index: number): void {
    const nextFundAssignments = getExplicitFundAssignments(
      fundAssignments,
    ).filter((_, assignmentIndex) => assignmentIndex !== index);
    setFundAssignments?.(
      updateUnassignedFundAmount(
        unassignedFund,
        totalAmountToAssign,
        nextFundAssignments,
      ),
    );
  };

  const updateFund = function (
    index: number,
    newFund: FundIdentifier | null,
  ): void {
    const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
    const nextFundAssignments = explicitFundAssignments.map(
      (assignment, assignmentIndex) => {
        if (assignmentIndex !== index) {
          return assignment;
        }
        if (newFund === null) {
          return {
            fundId: "",
            fundName: "",
            amount: assignment.amount,
            previousFundBalance: 0,
            newFundBalance: 0,
            previousGoalBalance: {
              remainingAmountToAssignIncludingPending: 0,
              remainingAmountToSpendIncludingPending: 0,
            },
            newGoalBalance: {
              remainingAmountToAssignIncludingPending: 0,
              remainingAmountToSpendIncludingPending: 0,
            },
          };
        }
        const fund = funds.find((f) => f.id === newFund.id);
        const previousFundBalance = fund?.currentBalance.postedBalance ?? 0;
        const goal = spendingGoals.find((g) => g.fundId === newFund.id);
        const previousGoalBalance =
          goal?.remainingAmountToSpendIncludingPending ?? 0;
        return {
          fundId: newFund.id,
          fundName: newFund.name,
          amount: assignment.amount,
          previousFundBalance,
          newFundBalance: previousFundBalance - assignment.amount,
          previousGoalBalance: {
            remainingAmountToAssignIncludingPending: 0,
            remainingAmountToSpendIncludingPending: previousGoalBalance,
          },
          newGoalBalance: {
            remainingAmountToAssignIncludingPending: 0,
            remainingAmountToSpendIncludingPending: Math.max(
              previousGoalBalance - assignment.amount,
              0,
            ),
          },
        };
      },
    );
    setFundAssignments?.(
      updateUnassignedFundAmount(
        unassignedFund,
        totalAmountToAssign,
        nextFundAssignments,
      ),
    );
  };

  const updateAmount = function (
    index: number,
    newAmount: number | null,
  ): void {
    const nextFundAssignments = getExplicitFundAssignments(fundAssignments).map(
      (assignment, assignmentIndex) => {
        if (assignmentIndex !== index) {
          return assignment;
        }
        const goal = spendingGoals.find((g) => g.fundId === assignment.fundId);
        const previousGoalBalance =
          goal?.remainingAmountToSpendIncludingPending ?? 0;
        return {
          ...assignment,
          amount: newAmount ?? 0,
          newFundBalance: assignment.previousFundBalance - (newAmount ?? 0),
          newGoalBalance: {
            remainingAmountToAssignIncludingPending: 0,
            remainingAmountToSpendIncludingPending: Math.max(
              previousGoalBalance - (newAmount ?? 0),
              0,
            ),
          },
        };
      },
    );
    setFundAssignments?.(
      updateUnassignedFundAmount(
        unassignedFund,
        totalAmountToAssign,
        nextFundAssignments,
      ),
    );
  };

  const renderAssignmentDetails = function (
    assignment: FundAssignmentDraft,
  ): JSX.Element | null {
    if (assignment.fundId === "") {
      return null;
    }
    return (
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1} useFlexGap>
        <Chip
          variant="outlined"
          label={`Previous remaining to spend ${formatCurrency(assignment.previousGoalBalance.remainingAmountToSpendIncludingPending)}`}
        />
        <Chip
          color={
            assignment.newGoalBalance.remainingAmountToSpendIncludingPending >=
            0
              ? "success"
              : "error"
          }
          label={`New remaining to spend ${formatCurrency(assignment.newGoalBalance.remainingAmountToSpendIncludingPending)}`}
        />
      </Stack>
    );
  };

  return (
    <FundAssignmentPlanner
      title="Spending Fund Assignment"
      funds={funds}
      totalAmountToAssign={totalAmountToAssign}
      fundAssignments={fundAssignments}
      addFundAssignment={addFundAssignment}
      deleteFundAssignment={deleteFundAssignment}
      updateFund={updateFund}
      updateAmount={updateAmount}
      remainingAmountLabel="Remaining"
      getRemainingAmountColor={(remainingAmount) => {
        if (remainingAmount === null) {
          return "default";
        }

        if (remainingAmount === 0) {
          return "success";
        }

        return "error";
      }}
      getFundOptionSecondaryLabel={(fund) =>
        getFundOptionSecondaryLabel(
          "Remaining to spend",
          getSpendingGoalRemainingAmount(
            fund.id,
            spendingGoals,
            baselineFundAssignments,
          ),
        )
      }
      sortFunds={sortFunds}
      renderAssignmentDetails={renderAssignmentDetails}
      color={frameColor}
      readOnly={readOnly}
    />
  );
};

export default SpendingFundAssignmentPlanner;
