import { AddCircleOutline, DeleteOutline } from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  LinearProgress,
  Paper,
  Stack,
  Typography,
  alpha,
} from "@mui/material";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import type { Fund, FundAmount, FundIdentifier } from "@/funds/types";
import {
  getAssignedFundAmount,
  getExplicitFundAssignments,
  getRemainingFundAmount,
  getUnassignedFund,
  hasOverAllocatedFundAssignments,
  updateUnassignedFundAmount,
} from "@/funds/fundAssignment";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

interface FundAssignmentPlannerProps {
  readonly title: string;
  readonly tone: "income" | "spending";
  readonly funds: Fund[];
  readonly assignmentGoals?: AssignmentGoal[];
  readonly spendingGoals?: SpendingGoal[];
  readonly totalAmountToAssign: number | null;
  readonly baselineValue?: FundAmount[];
  readonly value: FundAmount[];
  readonly setValue: (newValue: FundAmount[]) => void;
}

const emptyAssignmentGoals: AssignmentGoal[] = [];
const emptySpendingGoals: SpendingGoal[] = [];
const emptyFundAmounts: FundAmount[] = [];

/**
 * Presents a responsive fund allocation planner that keeps the unassigned remainder automatic.
 */
const FundAssignmentPlanner = function ({
  title,
  tone,
  funds,
  assignmentGoals = emptyAssignmentGoals,
  spendingGoals = emptySpendingGoals,
  totalAmountToAssign,
  baselineValue = emptyFundAmounts,
  value,
  setValue,
}: FundAssignmentPlannerProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);
  const explicitFundAssignments = getExplicitFundAssignments(
    unassignedFund,
    value,
  );
  const baselineAssignments = getExplicitFundAssignments(
    unassignedFund,
    baselineValue,
  );
  const assignedAmount = getAssignedFundAmount(unassignedFund, value);
  const remainingAmount = getRemainingFundAmount(
    unassignedFund,
    totalAmountToAssign,
    value,
  );
  const isOverAllocated = hasOverAllocatedFundAssignments(
    unassignedFund,
    totalAmountToAssign,
    value,
  );
  const assignmentGoalsByFundId = new Map(
    assignmentGoals.map((goal) => [goal.fundId, goal]),
  );
  const spendingGoalsByFundId = new Map(
    spendingGoals.map((goal) => [goal.fundId, goal]),
  );
  const baselineAssignedAmountsByFundId = new Map(
    baselineAssignments.map((assignment) => [
      assignment.fundId,
      assignment.amount,
    ]),
  );

  const availableFunds = funds.filter(
    (fund) =>
      fund.name !== "Unassigned" &&
      !explicitFundAssignments.some(
        (assignment) => assignment.fundId === fund.id,
      ),
  );

  const applyAssignments = function (nextAssignments: FundAmount[]): void {
    setValue(
      updateUnassignedFundAmount(
        unassignedFund,
        totalAmountToAssign,
        nextAssignments,
      ),
    );
  };

  const getGoalRemainingBeforeCurrentAssignment = function (
    fundId: string,
  ): number | null {
    const goal =
      tone === "income"
        ? assignmentGoalsByFundId.get(fundId)
        : spendingGoalsByFundId.get(fundId);

    if (typeof goal === "undefined") {
      return null;
    }

    const baselineAssignedAmount =
      baselineAssignedAmountsByFundId.get(fundId) ?? 0;

    return "remainingAmountToAssignIncludingPending" in goal
      ? goal.remainingAmountToAssignIncludingPending + baselineAssignedAmount
      : goal.remainingAmountToSpendIncludingPending + baselineAssignedAmount;
  };

  const getProjectedGoalRemainingAmount = function (
    fundId: string,
    amount: number,
  ): number | null {
    const goalRemainingBeforeCurrentAssignment =
      getGoalRemainingBeforeCurrentAssignment(fundId);

    if (goalRemainingBeforeCurrentAssignment === null) {
      return null;
    }

    return goalRemainingBeforeCurrentAssignment - amount;
  };

  const getFundOptionSecondaryLabel = function (
    fund: FundIdentifier,
  ): string | null {
    const goalRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
      fund.id,
    );

    if (goalRemainingAmount === null) {
      return "No goal";
    }

    return tone === "income"
      ? `Remaining to assign ${formatCurrency(goalRemainingAmount)}`
      : `Remaining to spend ${formatCurrency(goalRemainingAmount)}`;
  };

  const sortFundsByRemainingAmount = function (
    left: FundIdentifier,
    right: FundIdentifier,
  ): number {
    const leftRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
      left.id,
    );
    const rightRemainingAmount = getGoalRemainingBeforeCurrentAssignment(
      right.id,
    );

    if (leftRemainingAmount === null && rightRemainingAmount === null) {
      return left.name.localeCompare(right.name);
    }

    if (leftRemainingAmount === null) {
      return 1;
    }

    if (rightRemainingAmount === null) {
      return -1;
    }

    if (leftRemainingAmount !== rightRemainingAmount) {
      return rightRemainingAmount - leftRemainingAmount;
    }

    return left.name.localeCompare(right.name);
  };

  const getSuggestedAmount = function (index: number): number {
    if (totalAmountToAssign === null) {
      return explicitFundAssignments[index]?.amount ?? 0;
    }

    const amountAssignedElsewhere = explicitFundAssignments.reduce(
      (acc, assignment, assignmentIndex) =>
        assignmentIndex === index ? acc : acc + assignment.amount,
      0,
    );

    return Math.max(totalAmountToAssign - amountAssignedElsewhere, 0);
  };

  const updateAssignment = function (
    index: number,
    nextAssignment: FundAmount,
  ): void {
    const nextAssignments = [...explicitFundAssignments];
    nextAssignments[index] = nextAssignment;
    applyAssignments(nextAssignments);
  };

  const addAssignment = function (): void {
    const nextFund = availableFunds[0] ?? null;
    const nextAssignments = [
      ...explicitFundAssignments,
      {
        fundId: nextFund?.id ?? "",
        fundName: nextFund?.name ?? "",
        amount: remainingAmount ?? 0,
      },
    ];
    applyAssignments(nextAssignments);
  };

  const accentColor = tone === "income" ? "success.main" : "error.main";
  const progressValue =
    totalAmountToAssign !== null && totalAmountToAssign > 0
      ? Math.min((assignedAmount / totalAmountToAssign) * 100, 100)
      : 0;
  const overAllocatedAmount =
    totalAmountToAssign === null ? 0 : assignedAmount - totalAmountToAssign;

  return (
    <Paper
      variant="outlined"
      sx={{
        borderRadius: 4,
        p: { xs: 2, md: 3 },
        background: (theme) =>
          `linear-gradient(180deg, ${alpha(theme.palette.background.paper, 0.98)} 0%, ${alpha(theme.palette.background.default, 0.92)} 100%)`,
      }}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: "column", md: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", md: "center" }}
        >
          <Box>
            <Typography variant="h6">{title}</Typography>
            <Typography variant="body2" color="text.secondary">
              Pick each fund once and only adjust the portions you want to
              split. The remainder is tracked automatically.
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Chip label={`Total ${formatCurrency(totalAmountToAssign ?? 0)}`} />
            <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
            <Chip
              color={
                remainingAmount === null
                  ? "default"
                  : remainingAmount === 0
                    ? "success"
                    : tone === "spending"
                      ? "warning"
                      : "info"
              }
              label={
                tone === "income"
                  ? `Unassigned ${formatCurrency(remainingAmount ?? 0)}`
                  : `Remaining ${formatCurrency(remainingAmount ?? 0)}`
              }
            />
          </Stack>
        </Stack>

        <Box>
          <LinearProgress
            variant="determinate"
            value={progressValue}
            sx={{
              height: 10,
              borderRadius: 999,
              backgroundColor: (theme) => alpha(theme.palette.divider, 0.2),
              "& .MuiLinearProgress-bar": {
                borderRadius: 999,
                backgroundColor: accentColor,
              },
            }}
          />
        </Box>

        {totalAmountToAssign === null ? (
          <Alert severity="info">
            Enter the transaction amount first and the planner will keep the
            assignment totals in sync.
          </Alert>
        ) : null}
        {isOverAllocated ? (
          <Alert severity="error">
            Assigned funds exceed the transaction amount by{" "}
            {formatCurrency(overAllocatedAmount)}.
          </Alert>
        ) : null}
        {!isOverAllocated &&
        tone === "spending" &&
        remainingAmount !== null &&
        remainingAmount > 0 ? (
          <Alert severity="warning">
            Allocate the remaining {formatCurrency(remainingAmount)} before you
            save this spending transaction.
          </Alert>
        ) : null}
        {!isOverAllocated &&
        tone === "income" &&
        remainingAmount !== null &&
        remainingAmount > 0 ? (
          <Alert severity="info">
            {formatCurrency(remainingAmount)} will stay in the Unassigned fund
            unless you direct it elsewhere.
          </Alert>
        ) : null}

        <Stack spacing={2}>
          {explicitFundAssignments.length === 0 ? (
            <Box
              sx={{
                border: "1px dashed",
                borderColor: "divider",
                borderRadius: 3,
                p: 3,
                textAlign: "center",
              }}
            >
              <Typography variant="body1">
                No explicit fund assignments yet.
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Add a fund to direct part of this transaction away from the
                automatic remainder.
              </Typography>
            </Box>
          ) : null}

          {explicitFundAssignments.map((assignment, index) => {
            const projectedGoalRemainingAmount =
              getProjectedGoalRemainingAmount(
                assignment.fundId,
                assignment.amount,
              );

            return (
              <Paper
                key={assignment.fundId || `assignment-${index}`}
                variant="outlined"
                sx={{ borderRadius: 3, p: { xs: 2, md: 2.5 } }}
              >
                <Stack spacing={2}>
                  <Stack
                    direction={{ xs: "column", sm: "row" }}
                    spacing={1}
                    justifyContent="space-between"
                    alignItems={{ xs: "flex-start", sm: "center" }}
                  >
                    <Box>
                      <Typography variant="subtitle1">
                        Assignment {index + 1}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Suggested amount:{" "}
                        {formatCurrency(getSuggestedAmount(index))}
                      </Typography>
                    </Box>
                    <IconButton
                      aria-label="Delete fund assignment"
                      onClick={() => {
                        applyAssignments(
                          explicitFundAssignments.filter(
                            (_, itemIndex) => itemIndex !== index,
                          ),
                        );
                      }}
                    >
                      <DeleteOutline />
                    </IconButton>
                  </Stack>

                  <Box
                    sx={{
                      display: "grid",
                      gap: 2,
                      gridTemplateColumns:
                        "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
                    }}
                  >
                    <FundEntryField
                      label="Fund"
                      options={funds}
                      value={{
                        id: assignment.fundId,
                        name: assignment.fundName,
                      }}
                      setValue={(newValue): void => {
                        updateAssignment(index, {
                          fundId: newValue?.id ?? "",
                          fundName: newValue?.name ?? "",
                          amount:
                            assignment.amount > 0
                              ? assignment.amount
                              : getSuggestedAmount(index),
                        });
                      }}
                      filter={(fund) =>
                        fund.name !== "Unassigned" &&
                        (fund.id === assignment.fundId ||
                          !explicitFundAssignments.some(
                            (existingAssignment) =>
                              existingAssignment.fundId === fund.id,
                          ))
                      }
                      getOptionSecondaryLabel={getFundOptionSecondaryLabel}
                      sortComparator={sortFundsByRemainingAmount}
                    />
                    <CurrencyEntryField
                      label="Assigned Amount"
                      value={assignment.amount}
                      setValue={(newAmount): void => {
                        updateAssignment(index, {
                          ...assignment,
                          amount: newAmount ?? 0,
                        });
                      }}
                    />
                  </Box>

                  {assignment.fundId === "" ? (
                    <Typography variant="body2" color="text.secondary">
                      Choose a fund to see how this assignment changes its goal.
                    </Typography>
                  ) : projectedGoalRemainingAmount === null ? (
                    <Typography variant="body2" color="text.secondary">
                      No goal is set for this fund.
                    </Typography>
                  ) : (
                    <Stack
                      direction={{ xs: "column", sm: "row" }}
                      spacing={1}
                      useFlexGap
                    >
                      <Chip
                        variant="outlined"
                        label={`${tone === "income" ? "Remaining to assign" : "Remaining to spend"} ${formatCurrency(projectedGoalRemainingAmount - assignment.amount)}`}
                      />
                      <Chip
                        color={
                          projectedGoalRemainingAmount <= 0
                            ? "success"
                            : "default"
                        }
                        label={`${tone === "income" ? "New remaining to assign" : "New remaining to spend"} ${formatCurrency(projectedGoalRemainingAmount)}`}
                      />
                    </Stack>
                  )}

                  <Stack direction={{ xs: "column", sm: "row" }} spacing={1}>
                    <Button
                      variant="text"
                      onClick={() => {
                        updateAssignment(index, {
                          ...assignment,
                          amount: getSuggestedAmount(index),
                        });
                      }}
                    >
                      Use Suggested Amount
                    </Button>
                  </Stack>
                </Stack>
              </Paper>
            );
          })}
        </Stack>

        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
        >
          <Typography variant="body2" color="text.secondary">
            {availableFunds.length > 0
              ? `${availableFunds.length} fund${availableFunds.length === 1 ? "" : "s"} still available to assign.`
              : "All available funds are already represented in this split."}
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddCircleOutline />}
            onClick={addAssignment}
            disabled={availableFunds.length === 0}
          >
            Add Fund Assignment
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default FundAssignmentPlanner;
