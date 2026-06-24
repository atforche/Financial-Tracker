import type { JSX, ReactNode } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Box } from "@mui/material";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import TransactionSection from "@/transactions/workspace/TransactionSection";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";

interface TransactionDetailsViewSectionProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly date: string;
  readonly description: string;
  readonly amount: number;
  readonly headerAction?: ReactNode;
}

/**
 * Displays the shared read-only transaction details.
 */
const TransactionDetailsViewSection = function ({
  accountingPeriod,
  date,
  description,
  amount,
  headerAction,
}: TransactionDetailsViewSectionProps): JSX.Element {
  return (
    <TransactionSection
      title="Transaction Details"
      description="Review the high level information captured for this transaction."
      headerAction={headerAction}
    >
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
        }}
      >
        <TransactionDisplayField
          label="Accounting Period"
          value={accountingPeriod.name}
        />
        <TransactionDisplayField
          label="Date"
          value={dayjs(date).format("MMMM D, YYYY")}
        />
        <TransactionDisplayField
          label="Amount"
          value={formatCurrency(amount)}
        />
      </Box>
      <TransactionDisplayField label="Description" value={description} />
    </TransactionSection>
  );
};

export default TransactionDetailsViewSection;
