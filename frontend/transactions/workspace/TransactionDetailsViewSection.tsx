import type { JSX, ReactNode } from "react";
import type { AccountingPeriod } from "@/accounting-periods/types";
import { Box } from "@mui/material";
import Frame from "@/framework/view/Frame";
import TransactionDisplayField from "@/transactions/workspace/TransactionDisplayField";
import dayjs from "dayjs";

/**
 * Props for the TransactionDetailsViewSection component.
 */
interface TransactionDetailsViewSectionProps {
  readonly accountingPeriod: AccountingPeriod;
  readonly date: string;
  readonly description: string;
  readonly headerContent?: ReactNode;
}

/**
 * Displays the shared read-only transaction details.
 */
const TransactionDetailsViewSection = function ({
  accountingPeriod,
  date,
  description,
  headerContent,
}: TransactionDetailsViewSectionProps): JSX.Element {
  return (
    <Frame
      title="Transaction Details"
      headerContent={headerContent}
      color="info"
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
      </Box>
      <TransactionDisplayField label="Description" value={description} />
    </Frame>
  );
};

export default TransactionDetailsViewSection;
