import { Checkbox, FormControlLabel, Stack } from "@mui/material";
import type { Fund, FundBalanceEvent, FundWithBalance } from "@/funds/types";
import {
  type FundAssignmentDraft,
  addFundAssignment as appendFundAssignment,
  autoAssignIncomeFundAssignments,
  createFundAssignmentDraft,
  getContributionRemainingAmount,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getSuggestedAmount,
  getUnassignedFundAmount,
  deleteFundAssignment as removeFundAssignment,
  sortFundsByRemainingAmount,
  updateFundAssignment,
} from "@/funds/assignmentPlanner/helpers";
import type {
  FundGoalBalanceEvent,
  FundGoalWithProgress,
} from "@/fund-goals/types";
import { type JSX, useState } from "react";
import BalanceChangeChip from "@/framework/view/BalanceChangeChip";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import Toast from "@/framework/alerts/Toast";
import { compareCurrencyAmounts } from "@/framework/currencyHelpers";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the IncomeFundAssignmentPlanner component.
 */
interface IncomeFundAssignmentPlannerProps {
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly fundBalanceEvents?: FundBalanceEvent[];
  readonly fundGoalBalanceEvents?: FundGoalBalanceEvent[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments: FundAssignmentDraft[];
  readonly readOnly?: boolean;
}

const emptyFundBalanceEvents: FundBalanceEvent[] = [];
const emptyFundGoalBalanceEvents: FundGoalBalanceEvent[] = [];

/**
 * Displays the fund assignment planner for income transactions. Shows each fund's balance and goal
 * changes from its balance events once they have posted.
 */
const IncomeFundAssignmentPlanner = function ({
  funds,
  fundGoals,
  fundBalanceEvents = emptyFundBalanceEvents,
  fundGoalBalanceEvents = emptyFundGoalBalanceEvents,
  totalAmountToAssign,
  fundAssignments,
  setFundAssignments,
  baselineFundAssignments,
  readOnly = false,
}: IncomeFundAssignmentPlannerProps): JSX.Element {
  const [autoAssignMessage, setAutoAssignMessage] = useState<string | null>(
    null,
  );
  const unassignedFund = getUnassignedFund(funds);
  const storedUnassignedAssignment = fundAssignments.find(
    (assignment) => assignment.fundId === unassignedFund?.id,
  );
  const persistentUnassignedAssignment =
    unassignedFund === null || readOnly
      ? storedUnassignedAssignment
      : (storedUnassignedAssignment ?? {
          ...createFundAssignmentDraft(),
          fundId: unassignedFund.id,
          fundName: unassignedFund.name,
          amount:
            getUnassignedFundAmount(totalAmountToAssign, fundAssignments) ?? 0,
        });
  const plannerFundAssignments = readOnly
    ? fundAssignments
    : [
        ...(persistentUnassignedAssignment === undefined
          ? []
          : [persistentUnassignedAssignment]),
        ...getExplicitFundAssignments(fundAssignments),
      ];
  const assignmentIndexOffset =
    !readOnly && persistentUnassignedAssignment !== undefined ? 1 : 0;
  const getExplicitAssignmentIndex = (index: number): number =>
    index - assignmentIndexOffset;

  const hasPlannedMonthlyContribution = function (fundId: string): boolean {
    const fundGoal = fundGoals.find((goal) => goal.fund.id === fundId);
    return (
      fundGoal?.plannedMonthlyContribution !== null &&
      fundGoal?.plannedMonthlyContribution !== undefined
    );
  };

  const sortFunds = function (left: Fund, right: Fund): number {
    return sortFundsByRemainingAmount(left, right, (fundId: string) =>
      getContributionRemainingAmount(
        fundId,
        fundGoals,
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
        false,
      ),
    );
  };

  const autoAssign = function (): void {
    const nextFundAssignments = autoAssignIncomeFundAssignments(
      totalAmountToAssign,
      funds,
      fundGoals,
      baselineFundAssignments,
      unassignedFund,
    );
    const assignmentsChanged =
      nextFundAssignments.length !== plannerFundAssignments.length ||
      nextFundAssignments.some((assignment, index) => {
        const currentAssignment = plannerFundAssignments[index];
        return (
          assignment.fundId !== currentAssignment?.fundId ||
          assignment.amount !== currentAssignment.amount ||
          assignment.isExtraContribution !==
            currentAssignment.isExtraContribution
        );
      });

    if (!assignmentsChanged) {
      setAutoAssignMessage(
        "No changes were made. Assignments are already up to date, or no funds have a remaining amount to assign.",
      );
      return;
    }

    setFundAssignments?.(nextFundAssignments);
  };

  const deleteFundAssignment = function (index: number): void {
    setFundAssignments?.(
      removeFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
        getExplicitAssignmentIndex(index),
      ),
    );
  };

  const updateFund = function (index: number, newFund: Fund | null): void {
    setFundAssignments?.(
      updateFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
        getExplicitAssignmentIndex(index),
        (assignment) => {
          if (newFund === null) {
            return createFundAssignmentDraft(assignment.amount);
          }
          const recommendedAmount = getSuggestedAmount(
            totalAmountToAssign,
            fundAssignments,
            getExplicitAssignmentIndex(index),
            getContributionRemainingAmount(
              newFund.id,
              fundGoals,
              baselineFundAssignments,
            ) ?? 0,
          );
          return {
            fundId: newFund.id,
            fundName: newFund.name,
            amount: recommendedAmount,
            isExtraContribution:
              hasPlannedMonthlyContribution(newFund.id) &&
              assignment.isExtraContribution,
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
        getExplicitAssignmentIndex(index),
        (assignment) => ({ ...assignment, amount: newAmount ?? 0 }),
      ),
    );
  };

  const updateExtraContribution = function (
    index: number,
    isExtraContribution: boolean,
  ): void {
    setFundAssignments?.(
      updateFundAssignment(
        unassignedFund,
        totalAmountToAssign,
        fundAssignments,
        getExplicitAssignmentIndex(index),
        (assignment) => ({ ...assignment, isExtraContribution }),
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
    const goalEvent = fundGoalBalanceEvents.find(
      (candidate) => candidate.fund.id === assignment.fundId,
    );
    const previousBalance = balanceEvent?.previousBalance ?? null;
    const newBalance = balanceEvent?.newBalance ?? null;
    const previousTotals = goalEvent?.previousTotals ?? null;
    const newTotals = goalEvent?.newTotals ?? null;
    const showGoal =
      previousTotals !== null &&
      newTotals !== null &&
      hasPlannedMonthlyContribution(assignment.fundId);
    const showBalance = previousBalance !== null && newBalance !== null;
    if (!showGoal && !showBalance) {
      return null;
    }

    return (
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1} useFlexGap>
        {showGoal ? (
          <BalanceChangeChip
            label="Remaining to Assign"
            previousValue={
              previousTotals.remainingAmountToAssignToExpectedContribution
            }
            newValue={newTotals.remainingAmountToAssignToExpectedContribution}
            color={
              compareCurrencyAmounts(
                newTotals.remainingAmountToAssignToExpectedContribution,
                0,
              ) <= 0
                ? "success"
                : "default"
            }
          />
        ) : null}
        {showBalance ? (
          <BalanceChangeChip
            label="Balance"
            previousValue={previousBalance.postedBalance}
            newValue={newBalance.postedBalance}
            color={
              compareCurrencyAmounts(newBalance.postedBalance, 0) >= 0
                ? "success"
                : "error"
            }
          />
        ) : null}
      </Stack>
    );
  };

  const renderAssignmentControl = function (
    assignment: FundAssignmentDraft,
    index: number,
  ): JSX.Element | null {
    if (
      assignment.fundId === "" ||
      assignment.fundId === unassignedFund?.id ||
      (!hasPlannedMonthlyContribution(assignment.fundId) &&
        !assignment.isExtraContribution)
    ) {
      return null;
    }
    return (
      <FormControlLabel
        control={
          <Checkbox
            checked={assignment.isExtraContribution}
            disabled={readOnly}
            onChange={(event) => {
              updateExtraContribution(index, event.target.checked);
            }}
          />
        }
        label="Extra contribution"
        sx={{ flexShrink: 0, m: 0, whiteSpace: "nowrap" }}
      />
    );
  };

  return (
    <>
      <FundAssignmentPlanner
        funds={funds}
        totalAmountToAssign={totalAmountToAssign}
        fundAssignments={plannerFundAssignments}
        addFundAssignment={addFundAssignment}
        onAutoAssign={readOnly ? null : autoAssign}
        deleteFundAssignment={deleteFundAssignment}
        updateFund={updateFund}
        updateAmount={updateAmount}
        isAssignmentReadOnly={(assignment) =>
          assignment.fundId === unassignedFund?.id
        }
        isAssignmentDeletable={(assignment) =>
          assignment.fundId !== unassignedFund?.id
        }
        isFundSelectable={(fund) => fund.id !== unassignedFund?.id}
        getFundOptionSecondaryLabel={(fund) =>
          getFundOptionSecondaryLabel(
            "Remaining to assign",
            getContributionRemainingAmount(
              fund.id,
              fundGoals,
              baselineFundAssignments,
            ),
          )
        }
        sortFunds={sortFunds}
        renderAssignmentDetails={renderAssignmentDetails}
        renderAssignmentControl={renderAssignmentControl}
        readOnly={readOnly}
      />
      <Toast
        severity="info"
        open={autoAssignMessage !== null}
        autoHideDuration={5000}
        onClose={() => {
          setAutoAssignMessage(null);
        }}
      >
        {autoAssignMessage}
      </Toast>
    </>
  );
};

export default IncomeFundAssignmentPlanner;
