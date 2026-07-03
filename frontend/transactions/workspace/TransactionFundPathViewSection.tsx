import { Box } from "@mui/material";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import type { TransactionFund } from "@/transactions/transaction";

/**
 * Props for the TransactionFundPathViewSection component.
 */
interface TransactionFundPathViewSectionProps {
  readonly title: string;
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftFund: TransactionFund | null;
  readonly rightFund: TransactionFund | null;
}

/**
 * Displays the read-only fund path for a transaction.
 */
const TransactionFundPathViewSection = function ({
  title,
  leftLabel,
  rightLabel,
  leftFund,
  rightFund,
}: TransactionFundPathViewSectionProps): JSX.Element {
  return (
    <Frame title={title}>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 240px), 1fr))",
        }}
      >
        <TransactionDisplayField
          label={leftLabel}
          value={leftFund?.fundName ?? "None"}
          helperText={
            leftFund !== null ? (
              <TransactionBalanceDetails
                previousPostedBalance={
                  leftFund.previousFundBalance.postedBalance
                }
                newPostedBalance={leftFund.newFundBalance.postedBalance}
              />
            ) : null
          }
        />
        <TransactionDisplayField
          label={rightLabel}
          value={rightFund?.fundName ?? "None"}
          helperText={
            rightFund !== null ? (
              <TransactionBalanceDetails
                previousPostedBalance={
                  rightFund.previousFundBalance.postedBalance
                }
                newPostedBalance={rightFund.newFundBalance.postedBalance}
              />
            ) : null
          }
        />
      </Box>
    </Frame>
  );
};

export default TransactionFundPathViewSection;
