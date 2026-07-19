import { Chip, Stack } from "@mui/material";
import type { Fund, FundWithBalance } from "@/funds/types";
import {
  type FundAssignmentDraft,
  addFundAssignment as appendFundAssignment,
  createFundAssignmentDraft,
  getFundOptionSecondaryLabel,
  getIncomeGoalRemainingAmount,
  deleteFundAssignment as removeFundAssignment,
  sortFundsByRemainingAmount,
  updateFundAssignment,
} from "@/funds/assignmentPlanner/helpers";
import type { AssignmentGoal } from "@/goals/types";
import type { FrameColor } from "@/framework/view/Frame";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";
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

  const sortFunds = function (left: Fund, right: Fund): number {
    return sortFundsByRemainingAmount(left, right, (fundId: string) =>
      getIncomeGoalRemainingAmount(
        fundId,
        assignmentGoals,
        baselineFundAssignments,
      ),
    );
  };

  const addFundAssignment = function (): void {
    setFundAssignments?.(
      appendFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
      ),
    );
  };

  const deleteFundAssignment = function (index: number): void {
    setFundAssignments?.(
      removeFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
        index,
      ),
    );
  };

  const updateFund = function (index: number, newFund: Fund | null): void {
    setFundAssignments?.(
      updateFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
        index,
        (assignment) => {
          if (newFund === null) {
            return createFundAssignmentDraft(assignment.amount);
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
            newFundBalance: previousFundBalance + assignment.amount,
            previousGoalBalance: { remainingAmount: previousGoalBalance },
            newGoalBalance: {
              remainingAmount: Math.max(
                previousGoalBalance - assignment.amount,
                0,
              ),
            },
          };
        },
      ),
    );
  };

  const updateAmount = function (
    index: number,
    newAmount: number | null,
  ): void {
    setFundAssignments?.(
      updateFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
        index,
        (assignment) => {
          const goal = assignmentGoals.find(
            (g) => g.fund.id === assignment.fundId,
          );
          const previousGoalBalance =
            goal?.remainingAmountToAssignIncludingPending ?? 0;
          return {
            ...assignment,
            amount: newAmount ?? 0,
            newFundBalance: assignment.previousFundBalance + (newAmount ?? 0),
            newGoalBalance: {
              remainingAmount: Math.max(
                previousGoalBalance - (newAmount ?? 0),
                0,
              ),
            },
          };
        },
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
          label={`Previous remaining to assign ${formatCurrency(assignment.previousGoalBalance.remainingAmount)}`}
        />
        <Chip
          color={
            assignment.newGoalBalance.remainingAmount <= 0
              ? "success"
              : "default"
          }
          label={`New remaining to assign ${formatCurrency(assignment.newGoalBalance.remainingAmount)}`}
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
      renderAssignmentDetails={renderAssignmentDetails}
      color={frameColor}
      readOnly={readOnly}
    />
  );
};

export default IncomeFundAssignmentPlanner;
