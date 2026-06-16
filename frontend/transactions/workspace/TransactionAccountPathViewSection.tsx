import { Box } from "@mui/material";
import type { JSX } from "react";
import type { TransactionAccount } from "@/transactions/types";
import TransactionBalanceDetails from "@/transactions/workspace/TransactionBalanceDetails";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import formatCurrency from "@/framework/formatCurrency";

interface TransactionAccountPathViewSectionProps {
  readonly title: string;
  readonly description: string;
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftAccount: TransactionAccount | null;
  readonly rightAccount: TransactionAccount | null;
}

/**
 * Displays the read-only account path for a transaction.
 */
const TransactionAccountPathViewSection = function ({
  title,
  description,
  leftLabel,
  rightLabel,
  leftAccount,
  rightAccount,
}: TransactionAccountPathViewSectionProps): JSX.Element {
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
          value={leftAccount?.accountName ?? "None"}
          helperText={
            leftAccount !== null ? (
              <TransactionBalanceDetails
                previousPostedBalance={formatCurrency(
                  leftAccount.previousAccountBalance.postedBalance,
                )}
                newPostedBalance={formatCurrency(
                  leftAccount.newAccountBalance.postedBalance,
                )}
              />
            ) : null
          }
        />
        <TransactionDisplayField
          label={rightLabel}
          value={rightAccount?.accountName ?? "None"}
          helperText={
            rightAccount !== null ? (
              <TransactionBalanceDetails
                previousPostedBalance={formatCurrency(
                  rightAccount.previousAccountBalance.postedBalance,
                )}
                newPostedBalance={formatCurrency(
                  rightAccount.newAccountBalance.postedBalance,
                )}
              />
            ) : null
          }
        />
      </Box>
    </TransactionSection>
  );
};

export default TransactionAccountPathViewSection;
