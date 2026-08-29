import {
  AddCircleOutline,
  AutoFixHigh,
  DeleteOutline,
} from "@mui/icons-material";
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
import {
  type FundAssignmentDraft,
  getAssignedFundAmount,
  getAvailableFundCount,
  getExplicitFundAssignments,
  getRemainingFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import {
  compareCurrencyAmounts,
  formatCurrency,
} from "@/framework/currencyHelpers";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { Fund } from "@/funds/types";
import FundEntryField from "@/funds/FundEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import { isUnassignedFund } from "@/funds/helpers";

/**
 * Props for the FundAssignmentPlanner component.
 */
interface FundAssignmentPlannerProps {
  readonly funds: Fund[];
  readonly totalAmountToAssign: number | null;
  readonly fundAssignments: FundAssignmentDraft[];
  readonly addFundAssignment: () => void;
  readonly onAutoAssign?: (() => void) | null;
  readonly deleteFundAssignment: (index: number) => void;
  readonly updateFund: (index: number, newFund: Fund | null) => void;
  readonly updateAmount: (index: number, newAmount: number | null) => void;
  readonly remainingAmountLabel: string;
  readonly getRemainingAmountColor: (
    remainingAmount: number | null,
  ) => ChipProps["color"];
  readonly getFundOptionSecondaryLabel?: ((fund: Fund) => string | null) | null;
  readonly sortFunds?: ((left: Fund, right: Fund) => number) | null;
  readonly renderAssignmentDetails?:
    | ((assignment: FundAssignmentDraft, index: number) => JSX.Element | null)
    | null;
  readonly renderAssignmentControl?:
    | ((assignment: FundAssignmentDraft, index: number) => JSX.Element | null)
    | null;
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
  onAutoAssign = null,
  deleteFundAssignment,
  updateFund,
  updateAmount,
  remainingAmountLabel,
  getRemainingAmountColor,
  getFundOptionSecondaryLabel = null,
  sortFunds = null,
  renderAssignmentDetails = null,
  renderAssignmentControl = null,
  color = "info",
  readOnly = false,
}: FundAssignmentPlannerProps): JSX.Element {
  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  const assignedAmount = getAssignedFundAmount(fundAssignments);
  const remainingAmount = getRemainingFundAmount(
    totalAmountToAssign,
    fundAssignments,
  );
  const assignedFundIds = new Set(
    explicitFundAssignments.map((assignment) => assignment.fundId),
  );
  const availableFundCount = getAvailableFundCount(funds, fundAssignments);

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
            <InsetFrame key={assignment.fundId || `assignment-${index}`}>
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
                        description: "",
                      }}
                      setValue={
                        readOnly
                          ? null
                          : (newValue): void => {
                              updateFund(index, newValue);
                            }
                      }
                      filter={(fund) =>
                        !isUnassignedFund(fund.name) &&
                        (fund.id === assignment.fundId ||
                          !assignedFundIds.has(fund.id))
                      }
                      getOptionSecondaryLabel={getFundOptionSecondaryLabel}
                      sortComparator={sortFunds}
                    />
                  </Box>
                  <Box
                    sx={{
                      flex: 1,
                      minWidth: 0,
                      display: "flex",
                      alignItems: { xs: "flex-start", md: "center" },
                      gap: 1,
                    }}
                  >
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
                    {renderAssignmentControl?.(assignment, index) ?? null}
                  </Box>
                  {readOnly ? null : (
                    <IconButton
                      aria-label="Delete fund assignment"
                      sx={{
                        alignSelf: { xs: "flex-end", md: "center" },
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

                {renderAssignmentDetails?.(assignment, index) ?? null}
              </Stack>
            </InsetFrame>
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
              {availableFundCount > 0
                ? `${availableFundCount} fund${availableFundCount === 1 ? "" : "s"} still available to assign.`
                : "All available funds are already represented in this split."}
            </Typography>
            <Stack direction="row" spacing={1.5} justifyContent="flex-end">
              <Button
                variant="contained"
                startIcon={<AddCircleOutline />}
                onClick={addFundAssignment}
                disabled={availableFundCount === 0}
              >
                Add Fund Assignment
              </Button>
              {onAutoAssign === null ? null : (
                <Button
                  variant="outlined"
                  startIcon={<AutoFixHigh />}
                  onClick={onAutoAssign}
                  disabled={
                    totalAmountToAssign === null ||
                    compareCurrencyAmounts(totalAmountToAssign, 0) <= 0
                  }
                >
                  Auto-assign
                </Button>
              )}
            </Stack>
          </Stack>
        )}
      </Stack>
    </Frame>
  );
};

export default FundAssignmentPlanner;
