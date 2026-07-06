import { AddCircleOutline, DeleteOutline } from "@mui/icons-material";
import {
  Box,
  Button,
  Chip,
  type ChipProps,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import Frame, { type FrameColor } from "@/framework/view/Frame";
import type { Fund, FundIdentifier } from "@/funds/types";
import {
  type FundAssignmentDraft,
  getAssignedFundAmount,
  getAvailableFundsToAssign,
  getExplicitFundAssignments,
  getRemainingFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the FundAssignmentPlanner component.
 */
interface FundAssignmentPlannerProps {
  readonly funds: Fund[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly addFundAssignment: () => void;
  readonly deleteFundAssignment: (index: number) => void;
  readonly updateFund: (index: number, newFund: FundIdentifier | null) => void;
  readonly updateAmount: (index: number, newAmount: number | null) => void;
  readonly remainingAmountLabel: string;
  readonly getRemainingAmountColor: (
    remainingAmount: number | null,
  ) => ChipProps["color"];
  readonly getFundOptionSecondaryLabel?:
    ((fund: FundIdentifier) => string | null) | null;
  readonly sortFunds?:
    ((left: FundIdentifier, right: FundIdentifier) => number) | null;
  readonly renderAssignmentDetails?:
    ((assignment: FundAssignmentDraft) => JSX.Element | null) | null;
  readonly color?: FrameColor;
  readonly readOnly?: boolean;
}

/**
 * Displays a generic fund assignment planner.
 */
const FundAssignmentPlanner = function ({
  funds,
  totalAmountToAssign,
  fundAssignments,
  addFundAssignment,
  deleteFundAssignment,
  updateFund,
  updateAmount,
  remainingAmountLabel,
  getRemainingAmountColor,
  getFundOptionSecondaryLabel = null,
  sortFunds = null,
  renderAssignmentDetails = null,
  color = "info",
  readOnly = false,
}: FundAssignmentPlannerProps): JSX.Element {
  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  const assignedAmount = getAssignedFundAmount(fundAssignments);
  const remainingAmount = getRemainingFundAmount(
    totalAmountToAssign,
    fundAssignments,
  );
  const availableFunds = getAvailableFundsToAssign(funds, fundAssignments);

  return (
    <Frame
      title="Fund Assignments"
      color={color}
      headerContent={
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
          <Chip label={`Total ${formatCurrency(totalAmountToAssign ?? 0)}`} />
          <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
          <Chip
            color={getRemainingAmountColor(remainingAmount)}
            label={`${remainingAmountLabel} ${formatCurrency(remainingAmount ?? 0)}`}
          />
        </Stack>
      }
    >
      <Stack spacing={2.5}>
        <Stack spacing={2}>
          {explicitFundAssignments.map((assignment, index) => (
            <Stack key={assignment.fundId || `assignment-${index}`} spacing={2}>
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
                    setValue={
                      readOnly
                        ? null
                        : (newValue): void => {
                            updateFund(index, newValue);
                          }
                    }
                    filter={(fund) =>
                      fund.name !== "Unassigned" &&
                      (fund.id === assignment.fundId ||
                        !explicitFundAssignments.some(
                          (existingAssignment) =>
                            existingAssignment.fundId === fund.id,
                        ))
                    }
                    getOptionSecondaryLabel={getFundOptionSecondaryLabel}
                    sortComparator={sortFunds}
                  />
                </Box>
                <Box sx={{ flex: 1, minWidth: 0 }}>
                  <CurrencyEntryField
                    label="Assigned Amount"
                    value={assignment.amount}
                    setValue={
                      readOnly
                        ? null
                        : (newAmount): void => {
                            updateAmount(index, newAmount);
                          }
                    }
                  />
                </Box>
                {readOnly ? null : (
                  <IconButton
                    aria-label="Delete fund assignment"
                    sx={{
                      alignSelf: { xs: "flex-end", md: "flex-start" },
                      flexShrink: 0,
                    }}
                    onClick={() => {
                      deleteFundAssignment(index);
                    }}
                  >
                    <DeleteOutline />
                  </IconButton>
                )}
              </Box>

              {renderAssignmentDetails?.(assignment) ?? null}
            </Stack>
          ))}
        </Stack>

        {readOnly ? null : (
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
              onClick={addFundAssignment}
              disabled={availableFunds.length === 0}
            >
              Add Fund Assignment
            </Button>
          </Stack>
        )}
      </Stack>
    </Frame>
  );
};

export default FundAssignmentPlanner;
