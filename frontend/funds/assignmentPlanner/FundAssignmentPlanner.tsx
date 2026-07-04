import { AddCircleOutline, DeleteOutline } from "@mui/icons-material";
import type { AssignmentGoal, SpendingGoal } from "@/goals/types";
import {
  Box,
  Button,
  Chip,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import {
  getAssignedFundAmount,
  getAvailableFundsToAssign,
  getExplicitFundAssignments,
  getFundOptionSecondaryLabel,
  getProjectedGoalRemainingAmount,
  getRemainingFundAmount,
  getSuggestedAmount,
  sortFundsByRemainingAmount,
  updateUnassignedFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Frame from "@/framework/view/Frame";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";
import { getUnassignedFund } from "@/funds/helpers";

const emptyAssignmentGoals: AssignmentGoal[] = [];
const emptySpendingGoals: SpendingGoal[] = [];
const emptyFundAmounts: FundAmount[] = [];

/**
 * Props for the FundAssignmentPlanner component.
 */
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
  const assignedAmount = getAssignedFundAmount(unassignedFund, value);
  const remainingAmount = getRemainingFundAmount(
    unassignedFund,
    totalAmountToAssign,
    value,
  );
  const availableFunds = getAvailableFundsToAssign(funds, value);
  const applyAssignments = function (nextAssignments: FundAmount[]): void {
    setValue(
      updateUnassignedFundAmount(
        unassignedFund,
        totalAmountToAssign,
        nextAssignments,
      ),
    );
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
    const nextAssignments = [
      ...explicitFundAssignments,
      {
        fundId: "",
        fundName: "",
        amount: 0,
      },
    ];
    applyAssignments(nextAssignments);
  };

  return (
    <Frame
      title={title}
      color="success"
      headerContent={
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
                    ? "error"
                    : "info"
            }
            label={
              tone === "income"
                ? `Unassigned ${formatCurrency(remainingAmount ?? 0)}`
                : `Remaining ${formatCurrency(remainingAmount ?? 0)}`
            }
          />
        </Stack>
      }
    >
      <Stack spacing={2.5}>
        <Stack spacing={2}>
          {explicitFundAssignments.map((assignment, index) => {
            const projectedGoalRemainingAmount =
              getProjectedGoalRemainingAmount(
                tone,
                assignment.fundId,
                assignmentGoals,
                spendingGoals,
                baselineValue,
                assignment.amount,
              );

            return (
              <Paper
                key={assignment.fundId || `assignment-${index}`}
                variant="outlined"
                sx={{ borderRadius: 3, p: { xs: 2, md: 2.5 } }}
              >
                <Stack spacing={2}>
                  <Box
                    sx={{
                      display: "flex",
                      flexDirection: { xs: "column", md: "row" },
                      alignItems: { xs: "stretch", md: "flex-start" },
                      gap: 2,
                    }}
                  >
                    <Box sx={{ flex: 1, minWidth: 0 }}>
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
                                : getSuggestedAmount(
                                    tone,
                                    funds,
                                    value,
                                    assignmentGoals,
                                    spendingGoals,
                                    baselineValue,
                                    index,
                                    newValue?.id ?? "",
                                    totalAmountToAssign,
                                  ),
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
                        getOptionSecondaryLabel={(fundId) =>
                          getFundOptionSecondaryLabel(
                            tone,
                            fundId.id,
                            assignmentGoals,
                            spendingGoals,
                            baselineValue,
                          )
                        }
                        sortComparator={(left, right) =>
                          sortFundsByRemainingAmount(
                            tone,
                            left,
                            right,
                            assignmentGoals,
                            spendingGoals,
                            baselineValue,
                          )
                        }
                      />
                    </Box>
                    <Box sx={{ flex: 1, minWidth: 0 }}>
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
                    <IconButton
                      aria-label="Delete fund assignment"
                      sx={{
                        alignSelf: { xs: "flex-end", md: "flex-start" },
                        flexShrink: 0,
                      }}
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
                  </Box>

                  {assignment.fundId ===
                  "" ? null : projectedGoalRemainingAmount === null ? (
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
                        label={`${tone === "income" ? "Previous remaining to assign" : "Previous remaining to spend"} ${formatCurrency(projectedGoalRemainingAmount + assignment.amount)}`}
                      />
                      <Chip
                        color={
                          tone === "income"
                            ? projectedGoalRemainingAmount <= 0
                              ? "success"
                              : "default"
                            : projectedGoalRemainingAmount >= 0
                              ? "success"
                              : "error"
                        }
                        label={`${tone === "income" ? "New remaining to assign" : "New remaining to spend"} ${formatCurrency(projectedGoalRemainingAmount)}`}
                      />
                    </Stack>
                  )}
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
    </Frame>
  );
};

export default FundAssignmentPlanner;
