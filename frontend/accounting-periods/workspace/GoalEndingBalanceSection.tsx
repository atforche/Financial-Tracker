import { Divider, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
} from "@/framework/currencyHelpers";
import GoalDetailRow from "@/accounting-periods/workspace/GoalDetailRow";
import GoalRangeBar from "@/accounting-periods/workspace/GoalRangeBar";
import type { JSX } from "react";

/**
 * Props for the GoalEndingBalanceSection component.
 */
interface GoalEndingBalanceSectionProps {
  readonly balance: number;
  readonly minimumBalance: number;
  readonly maximumBalance?: number | null;
  readonly amountBelowMinimum: number;
  readonly amountAboveMaximum: number;
  readonly status: "BelowMinimum" | "WithinRange" | "AboveMaximum";
}

/**
 * Shows where a period's balance ended against its goal, stating the result in words.
 */
const GoalEndingBalanceSection = function ({
  balance,
  minimumBalance,
  maximumBalance = null,
  amountBelowMinimum,
  amountAboveMaximum,
  status,
}: GoalEndingBalanceSectionProps): JSX.Element {
  const isTarget =
    maximumBalance !== null &&
    compareCurrencyAmounts(minimumBalance, maximumBalance) === 0;
  const result = {
    BelowMinimum: {
      text: `${formatCurrency(amountBelowMinimum)} below ${isTarget ? "target" : "minimum"}`,
      color: "error.main",
    },
    AboveMaximum: {
      text: `${formatCurrency(amountAboveMaximum)} above ${isTarget ? "target" : "maximum"}`,
      color: "warning.main",
    },
    WithinRange: {
      text: isTarget ? "On target" : "Within range",
      color: "success.main",
    },
  }[status];
  return (
    <Stack spacing={1}>
      <Typography variant="subtitle2">Ending balance</Typography>
      <Typography variant="h5" component="p">
        {formatCurrency(balance)}
      </Typography>
      {isTarget ? null : (
        <GoalRangeBar
          minimum={minimumBalance}
          maximum={maximumBalance}
          balance={balance}
          color={result.color}
        />
      )}
      <Typography variant="body2" color={result.color}>
        {result.text}
      </Typography>
      <Divider />
      {isTarget ? (
        <GoalDetailRow label="Target" value={formatCurrency(minimumBalance)} />
      ) : (
        <>
          <GoalDetailRow
            label="Minimum"
            value={formatCurrency(minimumBalance)}
          />
          <GoalDetailRow
            label="Maximum"
            value={
              maximumBalance === null ? "None" : formatCurrency(maximumBalance)
            }
          />
        </>
      )}
    </Stack>
  );
};

export default GoalEndingBalanceSection;
