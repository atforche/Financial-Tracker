import { Box } from "@mui/material";
import type { JSX } from "react";
import type { TransactionAccount } from "@/transactions/types";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import dayjs from "dayjs";

interface TransactionAccountPathViewSectionProps {
  readonly title: string;
  readonly description: string;
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftAccount: TransactionAccount | null;
  readonly rightAccount: TransactionAccount | null;
}

const formatPostedDate = function (postedDate: string | null): string {
  return postedDate === null
    ? "Not posted"
    : dayjs(postedDate).format("MMMM D, YYYY");
};

/**
 * Displays the read-only account path for a transaction, including posted dates.
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
            leftAccount !== null
              ? `Posted: ${formatPostedDate(leftAccount.postedDate)}`
              : null
          }
        />
        <TransactionDisplayField
          label={rightLabel}
          value={rightAccount?.accountName ?? "None"}
          helperText={
            rightAccount !== null
              ? `Posted: ${formatPostedDate(rightAccount.postedDate)}`
              : null
          }
        />
      </Box>
    </TransactionSection>
  );
};

export default TransactionAccountPathViewSection;
