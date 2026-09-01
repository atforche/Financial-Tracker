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
import {
  type FundAssignmentDraft,
  getAssignedFundAmount,
  getAvailableFundCount,
  getExplicitFundAssignments,
  getRemainingFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import React, { useEffect, useState } from "react";
import {
  compareCurrencyAmounts,
  formatCurrency,
} from "@/framework/currencyHelpers";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { Fund } from "@/funds/types";
import FundEntryField from "@/funds/FundEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
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
  readonly showSummary?: boolean;
  readonly showTitle?: boolean;
  readonly persistentAssignment?: boolean;
  readonly addAssignmentInCard?: boolean;
  readonly fundLabel?: string;
  readonly getRemainingAmountColor: (
    remainingAmount: number | null,
  ) => ChipProps["color"];
  readonly getFundOptionSecondaryLabel?: ((fund: Fund) => string | null) | null;
  readonly sortFunds?: ((left: Fund, right: Fund) => number) | null;
  readonly renderAssignmentDetails?:
    | ((
        assignment: FundAssignmentDraft,
        index: number,
      ) => React.JSX.Element | null)
    | null;
  readonly renderAssignmentControl?:
    | ((
        assignment: FundAssignmentDraft,
        index: number,
      ) => React.JSX.Element | null)
    | null;
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
  showSummary = true,
  showTitle = true,
  persistentAssignment = false,
  addAssignmentInCard = false,
  fundLabel = "Fund",
  getRemainingAmountColor,
  getFundOptionSecondaryLabel = null,
  sortFunds = null,
  renderAssignmentDetails = null,
  renderAssignmentControl = null,
  readOnly = false,
}: FundAssignmentPlannerProps): React.JSX.Element {
  const [autoFocusAssignmentIndex, setAutoFocusAssignmentIndex] = useState<
    number | null
  >(null);
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
  const hasSinglePersistentAssignment =
    persistentAssignment && explicitFundAssignments.length === 1;

  useEffect(() => {
    if (autoFocusAssignmentIndex !== null) {
      setAutoFocusAssignmentIndex(null);
    }
  }, [autoFocusAssignmentIndex]);

  return (
    <Stack spacing={2.5}>
      {showSummary ? (
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
        >
          <Typography variant="subtitle1">Fund Assignments</Typography>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
            <Chip
              color={getRemainingAmountColor(remainingAmount)}
              label={`${remainingAmountLabel} ${formatCurrency(remainingAmount ?? 0)}`}
            />
          </Stack>
        </Stack>
      ) : showTitle ? (
        <Typography variant="subtitle1">Fund Assignments</Typography>
      ) : null}

      <Stack spacing={2}>
        {explicitFundAssignments.map((assignment, index) => (
          <InsetFrame key={`assignment-${index}`}>
            <Box
              sx={{
                display: "grid",
                gridTemplateColumns: {
                  xs: "minmax(0, 1fr)",
                  sm: "minmax(0, 1fr) minmax(0, 1fr)",
                },
                alignItems: "start",
                columnGap: { xs: 1, md: 2 },
                rowGap: 2,
              }}
            >
              <Box
                sx={{
                  minWidth: 0,
                  gridColumn: { xs: "1 / -1", sm: "auto" },
                  "& .combo-box-entry-field": { width: "100%" },
                }}
              >
                <FundEntryField
                  label={fundLabel}
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
                  autoFocus={autoFocusAssignmentIndex === index}
                />
              </Box>
              <Box
                sx={{
                  minWidth: 0,
                  display: "flex",
                  alignItems: "center",
                  gap: 1,
                }}
              >
                <Box
                  sx={{
                    flex: 1,
                    minWidth: 0,
                    "& .currency-entry-field": { width: "100%" },
                  }}
                >
                  <CurrencyEntryField
                    label="Assigned Amount"
                    value={
                      hasSinglePersistentAssignment
                        ? (totalAmountToAssign ?? 0)
                        : assignment.amount
                    }
                    setValue={
                      readOnly || hasSinglePersistentAssignment
                        ? null
                        : (newAmount): void => {
                            updateAmount(index, newAmount);
                          }
                    }
                  />
                </Box>
                {renderAssignmentControl?.(assignment, index) ?? null}
                {readOnly ? null : addAssignmentInCard && index === 0 ? (
                  <IconButton
                    aria-label="Add fund assignment"
                    size="small"
                    disabled={availableFundCount === 0}
                    sx={{ width: 32, height: 32, p: 0 }}
                    onClick={() => {
                      setAutoFocusAssignmentIndex(
                        explicitFundAssignments.length,
                      );
                      addFundAssignment();
                    }}
                  >
                    <AddCircleOutline />
                  </IconButton>
                ) : persistentAssignment && index === 0 ? null : (
                  <IconButton
                    aria-label="Delete fund assignment"
                    size="small"
                    sx={{ width: 32, height: 32, p: 0 }}
                    onClick={() => {
                      deleteFundAssignment(index);
                    }}
                  >
                    <DeleteOutline />
                  </IconButton>
                )}
              </Box>
              {renderAssignmentDetails === null ? null : (
                <Box
                  sx={{
                    gridColumn: { xs: "1", sm: "1 / 3" },
                    gridRow: { xs: "3", sm: "2" },
                  }}
                >
                  {renderAssignmentDetails(assignment, index)}
                </Box>
              )}
            </Box>
          </InsetFrame>
        ))}
      </Stack>

      {readOnly || (addAssignmentInCard && onAutoAssign === null) ? null : (
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
        >
          <Stack direction="row" spacing={1.5} justifyContent="flex-end">
            {addAssignmentInCard ? null : (
              <Button
                variant="contained"
                startIcon={<AddCircleOutline />}
                onClick={() => {
                  setAutoFocusAssignmentIndex(explicitFundAssignments.length);
                  addFundAssignment();
                }}
                disabled={availableFundCount === 0}
              >
                Add Fund Assignment
              </Button>
            )}
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
  );
};

export default FundAssignmentPlanner;
