import { Box } from "@mui/material";
import type { JSX } from "react";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import type { TransactionFund } from "@/transactions/transaction";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";

interface TransactionFundPathViewSectionProps {
  readonly title: string;
  readonly description: string;
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
  description,
  leftLabel,
  rightLabel,
  leftFund,
  rightFund,
}: TransactionFundPathViewSectionProps): JSX.Element {
  return (
    <TransactionSection title={title} description={description}>
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
                previousPostedBalance={formatCurrency(
                  leftFund.previousFundBalance.postedBalance,
                )}
                newPostedBalance={formatCurrency(
                  leftFund.newFundBalance.postedBalance,
                )}
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
                previousPostedBalance={formatCurrency(
                  rightFund.previousFundBalance.postedBalance,
                )}
                newPostedBalance={formatCurrency(
                  rightFund.newFundBalance.postedBalance,
                )}
              />
            ) : null
          }
        />
      </Box>
    </TransactionSection>
  );
};

export default TransactionFundPathViewSection;
