import type { Fund, FundBalanceEvent, FundWithBalance } from "@/funds/types";
import {
  type FundAssignmentDraft,
  addFundAssignment as appendFundAssignment,
  createFundAssignmentDraft,
  getEndingBalanceVariance,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getSpendingGoalRemainingAmount,
  deleteFundAssignment as removeFundAssignment,
  sortFundsByRemainingAmount,
  updateFundAssignment,
} from "@/funds/assignmentPlanner/helpers";
import { type JSX, useEffect } from "react";
import BalanceChangeChip from "@/framework/view/BalanceChangeChip";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import { Stack } from "@mui/material";
import { compareCurrencyAmounts } from "@/framework/currencyHelpers";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the SpendingFundAssignmentPlanner component.
 */
interface SpendingFundAssignmentPlannerProps {
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly fundBalanceEvents?: FundBalanceEvent[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly readOnly?: boolean;
}

const emptyFundBalanceEvents: FundBalanceEvent[] = [];

/**
 * Displays the fund assignment planner for spending transactions. Shows each fund's goal change from its
 * balance event once it has posted.
 */
const SpendingFundAssignmentPlanner = function ({
  funds,
  fundGoals,
  fundBalanceEvents = emptyFundBalanceEvents,
  totalAmountToAssign,
  fundAssignments,
  setFundAssignments,
  readOnly = false,
}: SpendingFundAssignmentPlannerProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);

  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  useEffect(() => {
    if (
      readOnly ||
      setFundAssignments === null ||
      explicitFundAssignments.length > 0
    ) {
      return;
    }

    setFundAssignments(
      appendFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
      ),
    );
  }, [
    explicitFundAssignments.length,
    fundAssignments,
    readOnly,
    setFundAssignments,
    totalAmountToAssign,
    unassignedFund,
  ]);

  const [singleExplicitAssignment] = explicitFundAssignments;
  const plannerFundAssignments =
    singleExplicitAssignment !== undefined &&
    explicitFundAssignments.length === 1 &&
    totalAmountToAssign !== null
      ? explicitFundAssignments.map((assignment) => ({
          ...assignment,
          amount: totalAmountToAssign,
        }))
      : explicitFundAssignments;

  const sortFunds = function (left: Fund, right: Fund): number {
    return sortFundsByRemainingAmount(left, right, (fundId: string) => {
      const fund = funds.find((candidate) => candidate.id === fundId);
      return getEndingBalanceVariance(
        fundId,
        fundGoals,
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
        false,
      ),
    );
  };

  const deleteFundAssignment = function (index: number): void {
    setFundAssignments?.(
      removeFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        plannerFundAssignments,
        index,
      ),
    );
  };

  const updateFund = function (index: number, newFund: Fund | null): void {
    setFundAssignments?.(
      updateFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        plannerFundAssignments,
        index,
        (assignment) => {
          if (newFund === null) {
            return createFundAssignmentDraft(assignment.amount);
          }
          return {
            fundId: newFund.id,
            fundName: newFund.name,
            amount: assignment.amount,
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
        plannerFundAssignments,
        index,
        (assignment) => ({ ...assignment, amount: newAmount ?? 0 }),
      ),
    );
  };

  const renderAssignmentDetails = function (
    assignment: FundAssignmentDraft,
  ): JSX.Element | null {
    if (assignment.fundId === "") {
      return null;
    }
    const balanceEvent = fundBalanceEvents.find(
      (candidate) => candidate.fund.id === assignment.fundId,
    );
    const previousBalance = balanceEvent?.previousBalance ?? null;
    const newBalance = balanceEvent?.newBalance ?? null;
    if (previousBalance === null || newBalance === null) {
      return null;
    }
    const newGoalAmount = getSpendingGoalRemainingAmount(
      assignment.fundId,
      fundGoals,
      newBalance.postedBalance,
    );
    return (
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1} useFlexGap>
        <BalanceChangeChip
          label="Remaining to Spend"
          previousValue={getSpendingGoalRemainingAmount(
            assignment.fundId,
            fundGoals,
            previousBalance.postedBalance,
          )}
          newValue={newGoalAmount}
          color={
            compareCurrencyAmounts(newGoalAmount, 0) >= 0 ? "success" : "error"
          }
        />
      </Stack>
    );
  };

  return (
    <FundAssignmentPlanner
      funds={funds}
      totalAmountToAssign={totalAmountToAssign}
      fundAssignments={plannerFundAssignments}
      addFundAssignment={addFundAssignment}
      deleteFundAssignment={deleteFundAssignment}
      updateFund={updateFund}
      updateAmount={updateAmount}
      isFundSelectable={(fund) => fund.id !== unassignedFund?.id}
      singleAssignmentAmountReadOnly
      fundLabel="Fund Assignment"
      getFundOptionSecondaryLabel={(fund) => {
        const fundWithBalance = funds.find(
          (candidate) => candidate.id === fund.id,
        );
        return getFundOptionSecondaryLabel(
          "Remaining to spend",
          getEndingBalanceVariance(
            fund.id,
            fundGoals,
            fundWithBalance?.currentBalance.postedBalance ?? 0,
          ),
        );
      }}
      sortFunds={sortFunds}
      renderAssignmentDetails={renderAssignmentDetails}
      readOnly={readOnly}
    />
  );
};

export default SpendingFundAssignmentPlanner;
