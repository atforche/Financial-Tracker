import { Box, Collapse, IconButton, Stack, Typography } from "@mui/material";
import {
  type FundAssignmentDraft,
  getAvailableFundCount,
  getExplicitFundAssignments,
} from "@/funds/assignmentPlanner/helpers";
import React, { useId, useState } from "react";
import { AutoFixHigh } from "@mui/icons-material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import CollectionItemDeleteButton from "@/framework/view/CollectionItemDeleteButton";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import ExpandMore from "@mui/icons-material/ExpandMore";
import type { Fund } from "@/funds/types";
import FundEntryField from "@/funds/FundEntryField";
import InsetFrame from "@/framework/view/InsetFrame";
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
  readonly persistentAssignment?: boolean;
  readonly singleAssignmentAmountReadOnly?: boolean;
  readonly isAssignmentReadOnly?:
    ((assignment: FundAssignmentDraft) => boolean) | null;
  readonly isAssignmentDeletable?:
    ((assignment: FundAssignmentDraft) => boolean) | null;
  readonly isFundSelectable?: ((fund: Fund) => boolean) | null;
  readonly collapsible?: boolean;
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
  persistentAssignment = false,
  singleAssignmentAmountReadOnly = false,
  isAssignmentReadOnly = null,
  isAssignmentDeletable = null,
  isFundSelectable = null,
  collapsible = false,
  fundLabel = "Fund",
  getFundOptionSecondaryLabel = null,
  sortFunds = null,
  renderAssignmentDetails = null,
  renderAssignmentControl = null,
  readOnly = false,
}: FundAssignmentPlannerProps): React.JSX.Element {
  const [assignmentsExpanded, setAssignmentsExpanded] = useState(false);
  const assignmentsDetailsId = useId();
  const explicitFundAssignments = getExplicitFundAssignments(fundAssignments);
  const assignedFundIds = new Set(
    explicitFundAssignments.map((assignment) => assignment.fundId),
  );
  const availableFundCount = getAvailableFundCount(funds, fundAssignments);
  const hasSinglePersistentAssignment =
    persistentAssignment && explicitFundAssignments.length === 1;
  const hasSingleAssignmentAmountReadOnly =
    singleAssignmentAmountReadOnly && explicitFundAssignments.length === 1;
  const assignmentCount = fundAssignments.length;
  const shouldCollapse = readOnly && collapsible && assignmentCount > 1;

  const assignmentsContent = (
    <CollectionEditor
      items={fundAssignments}
      onAdd={addFundAssignment}
      onRemove={(_, index) => {
        deleteFundAssignment(index);
      }}
      addLabel="Add another fund assignment"
      spacing={2}
      showAddButton={!readOnly && availableFundCount > 0}
      renderDeleteButton={(onRemove) => (
        <CollectionItemDeleteButton
          aria-label="Delete fund assignment"
          sx={{ width: 32, height: 32, p: 0 }}
          onClick={onRemove}
        />
      )}
      canDeleteItem={(assignment, index) =>
        !readOnly &&
        assignmentCount > 1 &&
        !(persistentAssignment && index === 0) &&
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
          !(persistentAssignment && index === 0) &&
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
                    value={
                      hasSinglePersistentAssignment
                        ? (totalAmountToAssign ?? 0)
                        : assignment.amount
                    }
                    setValue={
                      readOnly ||
                      assignmentReadOnly ||
                      hasSinglePersistentAssignment ||
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

  return (
    <Stack spacing={2.5}>
      {shouldCollapse ? (
        <InsetFrame>
          <Stack
            direction="row"
            alignItems="center"
            justifyContent="space-between"
          >
            <Typography variant="subtitle1">
              Fund Assignments ({assignmentCount})
            </Typography>
            <IconButton
              size="small"
              onClick={() => {
                setAssignmentsExpanded((expanded) => !expanded);
              }}
              aria-label={`${assignmentsExpanded ? "Collapse" : "Expand"} Fund Assignments`}
              aria-expanded={assignmentsExpanded}
              aria-controls={assignmentsDetailsId}
              sx={{
                p: 0.5,
                transform: assignmentsExpanded
                  ? "rotate(180deg)"
                  : "rotate(0deg)",
                transition: "transform 0.3s ease-in-out",
              }}
            >
              <ExpandMore />
            </IconButton>
          </Stack>
          <Collapse
            id={assignmentsDetailsId}
            in={assignmentsExpanded}
            timeout="auto"
            unmountOnExit
          >
            <Box sx={{ pt: 1.5 }}>{assignmentsContent}</Box>
          </Collapse>
        </InsetFrame>
      ) : (
        assignmentsContent
      )}
    </Stack>
  );
};

export default FundAssignmentPlanner;
