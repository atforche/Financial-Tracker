import { Checkbox, FormControlLabel, Stack } from "@mui/material";
import type { Fund, FundWithBalance } from "@/funds/types";
import {
  type FundAssignmentDraft,
  addFundAssignment as appendFundAssignment,
  autoAssignIncomeFundAssignments,
  createFundAssignmentDraft,
  getContributionRemainingAmount,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getRemainingFundAmount,
  getSuggestedAmount,
  deleteFundAssignment as removeFundAssignment,
  sortFundsByRemainingAmount,
  updateFundAssignment,
} from "@/funds/assignmentPlanner/helpers";
import { type JSX, useState } from "react";
import {
  compareCurrencyAmounts,
  getCurrencyTotal,
  getMaximumCurrencyAmount,
} from "@/framework/currencyHelpers";
import BalanceChangeChip from "@/framework/view/BalanceChangeChip";
import FundAssignmentPlanner from "@/funds/assignmentPlanner/FundAssignmentPlanner";
import type { FundGoalWithProgress } from "@/fund-goals/types";
import Toast from "@/framework/alerts/Toast";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the IncomeFundAssignmentPlanner component.
 */
interface IncomeFundAssignmentPlannerProps {
  readonly funds: FundWithBalance[];
  readonly fundGoals: FundGoalWithProgress[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly setFundAssignments:
    ((fundAssignments: FundAssignmentDraft[]) => void) | null;
  readonly baselineFundAssignments: FundAssignmentDraft[];
  readonly readOnly?: boolean;
}

/**
 * Displays the fund assignment planner for income transactions.
 */
const IncomeFundAssignmentPlanner = function ({
  funds,
  fundGoals,
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
  const unassignedFundWithBalance = funds.find(
    (fund) => fund.id === unassignedFund?.id,
  );
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
            getRemainingFundAmount(totalAmountToAssign, fundAssignments) ?? 0,
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
          const fund = funds.find((f) => f.id === newFund.id);
          const previousFundBalance = fund?.currentBalance.postedBalance ?? 0;
          const previousGoalAmount =
            getContributionRemainingAmount(
              newFund.id,
              fundGoals,
              baselineFundAssignments,
            ) ?? 0;
          const recommendedAmount = getSuggestedAmount(
            totalAmountToAssign,
            fundAssignments,
            getExplicitAssignmentIndex(index),
            previousGoalAmount,
          );
          const isExtraContribution =
            hasPlannedMonthlyContribution(newFund.id) &&
            assignment.isExtraContribution;
          return {
            fundId: newFund.id,
            fundName: newFund.name,
            amount: recommendedAmount,
            isExtraContribution,
            previousFundBalance,
            newFundBalance: getCurrencyTotal([
              previousFundBalance,
              recommendedAmount,
            ]),
            previousGoalAmount,
            newGoalAmount: getMaximumCurrencyAmount(
              getCurrencyTotal([
                previousGoalAmount,
                isExtraContribution ? 0 : -recommendedAmount,
              ]),
              0,
            ),
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
        (assignment) => ({
          ...assignment,
          amount: newAmount ?? 0,
          newFundBalance: getCurrencyTotal([
            assignment.previousFundBalance,
            newAmount ?? 0,
          ]),
          newGoalAmount: getMaximumCurrencyAmount(
            getCurrencyTotal([
              assignment.previousGoalAmount,
              assignment.isExtraContribution ? 0 : -(newAmount ?? 0),
            ]),
            0,
          ),
        }),
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
        (assignment) => ({
          ...assignment,
          isExtraContribution,
          newGoalAmount: getMaximumCurrencyAmount(
            getCurrencyTotal([
              assignment.previousGoalAmount,
              isExtraContribution ? 0 : -assignment.amount,
            ]),
            0,
          ),
        }),
      ),
    );
  };

  const renderAssignmentDetails = function (
    assignment: FundAssignmentDraft,
  ): JSX.Element | null {
    if (assignment.fundId === "") {
      return null;
    }
    const isSyntheticUnassignedAssignment =
      assignment.fundId === unassignedFund?.id &&
      !fundAssignments.includes(assignment);
    const previousFundBalance = isSyntheticUnassignedAssignment
      ? (unassignedFundWithBalance?.currentBalance.postedBalance ?? 0)
      : assignment.previousFundBalance;
    const newFundBalance = isSyntheticUnassignedAssignment
      ? getCurrencyTotal([previousFundBalance, assignment.amount])
      : assignment.newFundBalance;
    const previousGoalAmount = isSyntheticUnassignedAssignment
      ? 0
      : assignment.previousGoalAmount;
    const newGoalAmount = isSyntheticUnassignedAssignment
      ? assignment.amount
      : assignment.newGoalAmount;

    return (
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1} useFlexGap>
        <BalanceChangeChip
          label="Remaining to Assign"
          previousValue={previousGoalAmount}
          newValue={newGoalAmount}
          color={
            compareCurrencyAmounts(newGoalAmount, 0) <= 0
              ? "success"
              : "default"
          }
        />
        <BalanceChangeChip
          label="Balance"
          previousValue={previousFundBalance}
          newValue={newFundBalance}
          color={
            compareCurrencyAmounts(newFundBalance, 0) >= 0 ? "success" : "error"
          }
        />
      </Stack>
    );
  };

  const renderAssignmentControl = function (
    assignment: FundAssignmentDraft,
    index: number,
  ): JSX.Element | null {
    if (
      assignment.fundId === "" ||
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
        collapsible={readOnly}
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
