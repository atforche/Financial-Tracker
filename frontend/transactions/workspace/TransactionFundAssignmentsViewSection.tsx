import { Box, Chip, Paper, Stack, Typography } from "@mui/material";
import {
  getAssignedFundAmount,
  getRemainingFundAmount,
} from "@/funds/assignmentPlanner/helpers";
import Frame from "@/framework/view/Frame";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import type { TransactionFund } from "@/transactions/transaction";
import formatCurrency from "@/framework/formatCurrency";
import { getUnassignedFund } from "@/funds/helpers";

/**
 * Props for the TransactionFundAssignmentsViewSection component.
 */
interface TransactionFundAssignmentsViewSectionProps {
  readonly funds: Fund[];
  readonly amount: number;
  readonly fundAssignments: TransactionFund[];
  readonly tone: "income" | "spending";
}

/**
 * Displays the read-only fund allocation details for income and spending transactions.
 */
const TransactionFundAssignmentsViewSection = function ({
  funds,
  amount,
  fundAssignments,
  tone,
}: TransactionFundAssignmentsViewSectionProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);
  const explicitFundAssignments = fundAssignments.filter(
    (fundAssignment) =>
      fundAssignment.fundId !== unassignedFund?.id &&
      fundAssignment.fundName !== "Unassigned",
  );
  const assignedAmount = getAssignedFundAmount(fundAssignments);
  const remainingAmount = getRemainingFundAmount(amount, fundAssignments);

  return (
    <Frame
      title="Fund Allocation"
      color={tone === "income" ? "success" : "warning"}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: "column", md: "row" }}
          spacing={1}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", md: "center" }}
        >
          <Typography variant="body2" color="text.secondary">
            Explicit assignments are listed below. The remainder stays with the
            {tone === "income" ? " Unassigned fund" : " remaining amount"}.
          </Typography>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Chip label={`Total ${formatCurrency(amount)}`} />
            <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
            <Chip
              color={remainingAmount === 0 ? "success" : "info"}
              label={
                tone === "income"
                  ? `Unassigned ${formatCurrency(remainingAmount ?? 0)}`
                  : `Remaining ${formatCurrency(remainingAmount ?? 0)}`
              }
            />
          </Stack>
        </Stack>

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
              No explicit fund assignments.
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {tone === "income"
                ? "This transaction remains fully in the Unassigned fund."
                : "This transaction does not split any amount into named funds."}
            </Typography>
          </Box>
        ) : (
          <Box
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns:
                "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
            }}
          >
            {explicitFundAssignments.map((assignment) => (
              <Paper
                key={assignment.fundId}
                variant="outlined"
                sx={{ borderRadius: 3, p: { xs: 2, md: 2.5 } }}
              >
                <Stack spacing={1}>
                  <Typography variant="subtitle1">
                    {assignment.fundName}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Assigned Amount
                  </Typography>
                  <Typography variant="h6">
                    {formatCurrency(assignment.amount)}
                  </Typography>
                  <TransactionBalanceDetails
                    previousPostedBalance={
                      assignment.previousFundBalance.postedBalance
                    }
                    newPostedBalance={assignment.newFundBalance.postedBalance}
                  />
                </Stack>
              </Paper>
            ))}
          </Box>
        )}
      </Stack>
    </Frame>
  );
};

export default TransactionFundAssignmentsViewSection;
