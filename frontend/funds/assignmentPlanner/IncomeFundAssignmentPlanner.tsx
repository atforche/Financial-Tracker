import { Chip, Stack } from "@mui/material";
import type { FundIdentifier, FundWithBalance } from "@/funds/types";
import {
  type FundAssignmentDraft,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getIncomeGoalRemainingAmount,
  getSuggestedAmount,
  sortFundsByRemainingAmount,
  updateUnassignedFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import type { AssignmentGoal } from "@/goals/types";
import type { FrameColor } from "@/framework/view/Frame";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the IncomeFundAssignmentPlanner component.
 */
interface IncomeFundAssignmentPlannerProps {
  readonly funds: FundWithBalance[];
  readonly assignmentGoals: AssignmentGoal[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments: FundAssignmentDraft[];
  readonly frameColor?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays the fund assignment planner for income transactions.
 */
const IncomeFundAssignmentPlanner = function ({
  funds,
  assignmentGoals,
  totalAmountToAssign,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments,
  frameColor = "primary",
  readOnly = false,
}: IncomeFundAssignmentPlannerProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);

  const sortFunds = function (
    left: FundIdentifier,
    right: FundIdentifier,
  ): number {
    return sortFundsByRemainingAmount(left, right, (fundId: string) =>
      getIncomeGoalRemainingAmount(
        fundId,
        assignmentGoals,
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
        const goal = assignmentGoals.find((g) => g.fund.id === newFund.id);
        const previousGoalBalance =
          goal?.remainingAmountToAssignIncludingPending ?? 0;
        return {
          fundId: newFund.id,
          fundName: newFund.name,
          amount: assignment.amount,
          previousFundBalance,
          newFundBalance: Math.min(previousFundBalance - assignment.amount, 0),
          previousGoalBalance: {
            remainingAmountToAssignIncludingPending: previousGoalBalance,
            remainingAmountToSpendIncludingPending: 0,
          },
          newGoalBalance: {
            remainingAmountToAssignIncludingPending: Math.max(
              previousGoalBalance - assignment.amount,
              0,
            ),
            remainingAmountToSpendIncludingPending: 0,
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
        const goal = assignmentGoals.find(
          (g) => g.fund.id === assignment.fundId,
        );
        const previousGoalBalance =
          goal?.remainingAmountToAssignIncludingPending ?? 0;
        return {
          ...assignment,
          amount: newAmount ?? 0,
          newFundBalance: Math.min(
            assignment.previousFundBalance - (newAmount ?? 0),
            0,
          ),
          newGoalBalance: {
            remainingAmountToAssignIncludingPending: Math.max(
              previousGoalBalance - (newAmount ?? 0),
              0,
            ),
            remainingAmountToSpendIncludingPending: 0,
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
          label={`Previous remaining to assign ${formatCurrency(assignment.previousGoalBalance.remainingAmountToAssignIncludingPending)}`}
        />
        <Chip
          color={
            assignment.newGoalBalance.remainingAmountToAssignIncludingPending <=
            0
              ? "success"
              : "default"
          }
          label={`New remaining to assign ${formatCurrency(assignment.newGoalBalance.remainingAmountToAssignIncludingPending)}`}
        />
        <Chip
          variant="outlined"
          label={`Previous balance ${formatCurrency(assignment.previousFundBalance)}`}
        />
        <Chip
          color={assignment.newFundBalance >= 0 ? "success" : "error"}
          label={`New balance ${formatCurrency(assignment.newFundBalance)}`}
        />
      </Stack>
    );
  };

  return (
    <FundAssignmentPlanner
      funds={funds}
      totalAmountToAssign={totalAmountToAssign}
      fundAssignments={fundAssignments}
      addFundAssignment={addFundAssignment}
      deleteFundAssignment={deleteFundAssignment}
      updateFund={updateFund}
      updateAmount={updateAmount}
      remainingAmountLabel="Unassigned"
      getRemainingAmountColor={(remainingAmount) => {
        if (remainingAmount === null) {
          return "default";
        }
        return remainingAmount === 0 ? "success" : "info";
      }}
      getFundOptionSecondaryLabel={(fund) =>
        getFundOptionSecondaryLabel(
          "Remaining to assign",
          getIncomeGoalRemainingAmount(
            fund.id,
            assignmentGoals,
            baselineFundAssignments,
          ),
        )
      }
      sortFunds={sortFunds}
      renderAssignmentDetails={readOnly ? null : renderAssignmentDetails}
      color={frameColor}
      readOnly={readOnly}
    />
  );
};

export default IncomeFundAssignmentPlanner;
