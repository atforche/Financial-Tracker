import { Chip, Stack } from "@mui/material";
import type { Fund, FundWithBalance } from "@/funds/types";
import {
  type FundAssignmentDraft,
  addFundAssignment as appendFundAssignment,
  createFundAssignmentDraft,
  getEndingBalanceVariance,
  getFundOptionSecondaryLabel,
  getSpendingGoalRemainingAmount,
  deleteFundAssignment as removeFundAssignment,
  sortFundsByRemainingAmount,
  updateFundAssignment,
} from "@/funds/assignmentPlanner/helpers";
import type { FrameColor } from "@/framework/view/Frame";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import type { JSX } from "react";
import { formatCurrency } from "@/framework/currencyHelpers";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the SpendingFundAssignmentPlanner component.
 */
interface SpendingFundAssignmentPlannerProps {
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
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
  fundGoals,
  totalAmountToAssign,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments,
  frameColor = "primary",
  readOnly = false,
}: SpendingFundAssignmentPlannerProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);

  const sortFunds = function (left: Fund, right: Fund): number {
    return sortFundsByRemainingAmount(left, right, (fundId: string) => {
      const fund = funds.find((candidate) => candidate.id === fundId);
      return getEndingBalanceVariance(
        fundId,
        fundGoals,
        baselineFundAssignments,
        fund?.currentBalance.postedBalance ?? 0,
      );
    });
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
          const previousGoalAmount = getSpendingGoalRemainingAmount(
            newFund.id,
            fundGoals,
            previousFundBalance,
          );
          return {
            fundId: newFund.id,
            fundName: newFund.name,
            amount: assignment.amount,
            previousFundBalance,
            newFundBalance: previousFundBalance - assignment.amount,
            previousGoalAmount,
            newGoalAmount: previousGoalAmount - assignment.amount,
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
          const previousGoalAmount = getSpendingGoalRemainingAmount(
            assignment.fundId,
            fundGoals,
            assignment.previousFundBalance,
          );
          return {
            ...assignment,
            amount: newAmount ?? 0,
            newFundBalance: assignment.previousFundBalance - (newAmount ?? 0),
            newGoalAmount: previousGoalAmount - (newAmount ?? 0),
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
          label={`Previous remaining to spend ${formatCurrency(assignment.previousGoalAmount)}`}
        />
        <Chip
          color={assignment.newGoalAmount >= 0 ? "success" : "error"}
          label={`New remaining to spend ${formatCurrency(assignment.newGoalAmount)}`}
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
      getFundOptionSecondaryLabel={(fund) => {
        const fundWithBalance = funds.find(
          (candidate) => candidate.id === fund.id,
        );
        return getFundOptionSecondaryLabel(
          "Remaining to spend",
          getEndingBalanceVariance(
            fund.id,
            fundGoals,
            baselineFundAssignments,
            fundWithBalance?.currentBalance.postedBalance ?? 0,
          ),
        );
      }}
      sortFunds={sortFunds}
      renderAssignmentDetails={renderAssignmentDetails}
      color={frameColor}
      readOnly={readOnly}
    />
  );
};

export default SpendingFundAssignmentPlanner;
