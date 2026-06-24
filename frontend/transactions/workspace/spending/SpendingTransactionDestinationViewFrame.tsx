import { Box, Chip, Paper, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import type {
  TransactionAccount,
  TransactionFund,
} from "@/transactions/transaction";
import {
  getAssignedFundAmount,
  getRemainingFundAmount,
  getUnassignedFund,
} from "@/funds/fundAssignment";
import type { Fund } from "@/funds/types";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";
import formatCurrency from "@/framework/formatCurrency";

interface SpendingTransactionDestinationViewFrameProps {
  readonly index: number;
  readonly funds: Fund[];
  readonly account: TransactionAccount | null;
  readonly location: string | null;
  readonly amount: number;
  readonly fundAssignments: TransactionFund[];
  readonly helperContent?: ReactNode;
}

/**
 * Displays the read-only destination frame for one spending destination.
 */
const SpendingTransactionDestinationViewFrame = function ({
  index,
  funds,
  account,
  location,
  amount,
  fundAssignments,
  helperContent = null,
}: SpendingTransactionDestinationViewFrameProps): JSX.Element {
  const unassignedFund = getUnassignedFund(funds);
  const explicitFundAssignments = fundAssignments.filter(
    (fundAssignment) =>
      fundAssignment.fundId !== unassignedFund?.id &&
      fundAssignment.fundName !== "Unassigned",
  );
  const assignedAmount = getAssignedFundAmount(unassignedFund, fundAssignments);
  const remainingAmount = getRemainingFundAmount(
    unassignedFund,
    amount,
    fundAssignments,
  );
  const hasLocation = account === null && (location ?? "").trim() !== "";

  return (
    <TransactionFrame title={`Destination ${index + 1}`} description="">
      <Stack spacing={2}>
        <TransactionDisplayField
          label="Account"
          value={account?.accountName ?? "None"}
          helperText={
            account === null ? null : (
              <Stack spacing={1.25}>
                <TransactionBalanceDetails
                  previousPostedBalance={formatCurrency(
                    account.previousAccountBalance.postedBalance,
                  )}
                  newPostedBalance={formatCurrency(
                    account.newAccountBalance.postedBalance,
                  )}
                />
                {helperContent}
              </Stack>
            )
          }
        />

        {hasLocation ? (
          <>
            <Typography
              variant="body2"
              color="text.secondary"
              textAlign="center"
              textTransform="uppercase"
              letterSpacing="0.08em"
            >
              or
            </Typography>
            <TransactionDisplayField
              label="Destination Location"
              value={location ?? ""}
            />
          </>
        ) : null}

        <TransactionDisplayField
          label="Destination Amount"
          value={formatCurrency(amount)}
        />

        <Typography variant="subtitle1">Fund Assignments</Typography>
        <Stack spacing={2.5}>
          <Stack
            direction={{ xs: "column", md: "row" }}
            spacing={1}
            justifyContent="space-between"
            alignItems={{ xs: "flex-start", md: "center" }}
          >
            <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
              <Chip label={`Total ${formatCurrency(amount)}`} />
              <Chip label={`Assigned ${formatCurrency(assignedAmount)}`} />
              <Chip
                color={remainingAmount === 0 ? "success" : "info"}
                label={`Remaining ${formatCurrency(remainingAmount ?? 0)}`}
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
                This transaction does not split any amount into named funds.
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
                      {assignment.fundName} {formatCurrency(assignment.amount)}
                    </Typography>
                    <Stack
                      spacing={1}
                      direction="row"
                      justifyContent="space-between"
                    >
                      <TransactionBalanceDetails
                        previousPostedBalance={formatCurrency(
                          assignment.previousFundBalance.postedBalance,
                        )}
                        newPostedBalance={formatCurrency(
                          assignment.newFundBalance.postedBalance,
                        )}
                      />
                    </Stack>
                  </Stack>
                </Paper>
              ))}
            </Box>
          )}
        </Stack>
      </Stack>
    </TransactionFrame>
  );
};

export default SpendingTransactionDestinationViewFrame;
