import { Box, IconButton } from "@mui/material";
import {
  type FundAssignmentDraft,
  getAvailableFundCount,
  getExplicitFundAssignments,
} from "@/funds/assignmentPlanner/helpers";
import { AutoFixHigh } from "@mui/icons-material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import CollectionItemDeleteButton from "@/framework/view/CollectionItemDeleteButton";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { Fund } from "@/funds/types";
import FundEntryField from "@/funds/FundEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
import React from "react";
import { compareCurrencyAmounts } from "@/framework/currencyHelpers";

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
  readonly singleAssignmentAmountReadOnly?: boolean;
  readonly isAssignmentReadOnly?:
    ((assignment: FundAssignmentDraft) => boolean) | null;
  readonly isAssignmentDeletable?:
    ((assignment: FundAssignmentDraft) => boolean) | null;
  readonly isFundSelectable?: ((fund: Fund) => boolean) | null;
  readonly fundLabel?: string;
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
  singleAssignmentAmountReadOnly = false,
  isAssignmentReadOnly = null,
  isAssignmentDeletable = null,
  isFundSelectable = null,
  fundLabel = "Fund",
  getFundOptionSecondaryLabel = null,
  sortFunds = null,
  renderAssignmentDetails = null,
  renderAssignmentControl = null,
  readOnly = false,
}: FundAssignmentPlannerProps): React.JSX.Element {
  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  const assignedFundIds = new Set(
    explicitFundAssignments.map((assignment) => assignment.fundId),
  );
  const availableFundCount = getAvailableFundCount(funds, fundAssignments);
  const hasSingleAssignmentAmountReadOnly =
    singleAssignmentAmountReadOnly && explicitFundAssignments.length === 1;
  const assignmentCount = fundAssignments.length;

  const assignmentsContent = (
    <CollectionEditor
      items={fundAssignments}
      label="Fund Assignments"
      onAdd={addFundAssignment}
      onRemove={(_, index) => {
        deleteFundAssignment(index);
      }}
      addLabel="Add another fund assignment"
      spacing={2}
      showAddButton={!readOnly && availableFundCount > 0}
      renderDeleteButton={(onRemove) => (
        <CollectionItemDeleteButton
          sx={{ width: 32, height: 32, p: 0 }}
          onClick={onRemove}
        />
      )}
      canDeleteItem={(assignment) =>
        !readOnly &&
        assignmentCount > 1 &&
        isAssignmentDeletable?.(assignment) !== false
      }
      readOnly={readOnly}
      renderItem={(assignment, index, controls) => {
        const assignmentDetails =
          renderAssignmentDetails?.(assignment, index) ?? null;
        const assignmentReadOnly =
          readOnly || (isAssignmentReadOnly?.(assignment) ?? false);
        const assignmentDeletable =
          !readOnly &&
          assignmentCount > 1 &&
          isAssignmentDeletable?.(assignment) !== false;
        const showAutoAssign =
          !readOnly && onAutoAssign !== null && index === 0;

        return (
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
                    assignmentReadOnly
                      ? null
                      : (newValue): void => {
                          updateFund(index, newValue);
                        }
                  }
                  filter={(fund) =>
                    (isFundSelectable?.(fund) ?? true) &&
                    (fund.id === assignment.fundId ||
                      !assignedFundIds.has(fund.id))
                  }
                  getOptionSecondaryLabel={getFundOptionSecondaryLabel}
                  sortComparator={sortFunds}
                  autoFocus={controls.autoFocus}
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
                    value={assignment.amount}
                    setValue={
                      readOnly ||
                      assignmentReadOnly ||
                      hasSingleAssignmentAmountReadOnly
                        ? null
                        : (newAmount): void => {
                            updateAmount(index, newAmount);
                          }
                    }
                  />
                </Box>
                {renderAssignmentControl?.(assignment, index) ?? null}
                {showAutoAssign ? (
                  <IconButton
                    aria-label="Auto-assign fund assignments"
                    title="Auto-assign fund assignments"
                    size="small"
                    disabled={
                      totalAmountToAssign === null ||
                      compareCurrencyAmounts(totalAmountToAssign, 0) <= 0
                    }
                    sx={{ width: 32, height: 32, p: 0 }}
                    onClick={onAutoAssign}
                  >
                    <AutoFixHigh />
                  </IconButton>
                ) : null}
                {assignmentDeletable ? (
                  controls.deleteButton
                ) : !readOnly && index === 0 && !showAutoAssign ? (
                  <Box
                    aria-hidden="true"
                    sx={{ width: 32, height: 32, flexShrink: 0 }}
                  />
                ) : null}
              </Box>
              {assignmentDetails === null ? null : (
                <Box
                  sx={{
                    gridColumn: { xs: "1", sm: "1 / 3" },
                    gridRow: { xs: "3", sm: "2" },
                  }}
                >
                  {assignmentDetails}
                </Box>
              )}
            </Box>
          </InsetFrame>
        );
      }}
    />
  );

  return assignmentsContent;
};

export default FundAssignmentPlanner;
