import { Box } from "@mui/material";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSection from "@/transactions/workspace/TransactionSection";

interface TransactionFundPathViewSectionProps {
  readonly title: string;
  readonly description: string;
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftFund: Fund | null;
  readonly rightFund: Fund | null;
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
          value={leftFund?.name ?? "None"}
        />
        <TransactionDisplayField
          label={rightLabel}
          value={rightFund?.name ?? "None"}
        />
      </Box>
    </TransactionSection>
  );
};

export default TransactionFundPathViewSection;
