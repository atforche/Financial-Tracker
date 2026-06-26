import type {
  IncomeDeduction,
  IncomeLine,
} from "@/transactions/incomeTransaction";
import { Box } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import IncomeTransactionItemSection from "@/transactions/workspace/income/IncomeTransactionSourceItemFrame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import type { TransactionAccount } from "@/transactions/transaction";
import TransactionAccountViewDisplay from "@/transactions/workspace/TransactionAccountViewDisplay";
import TransactionFrame from "@/transactions/workspace/TransactionFrame";

/**
 * Props for the IncomeTransactionSourceViewFrame component.
 */
interface IncomeTransactionSourceViewFrameProps {
  readonly account: TransactionAccount | null;
  readonly location: string | null;
  readonly incomeLines: IncomeLine[];
  readonly incomeDeductions: IncomeDeduction[];
}

/**
 * Displays a view frame for an income transaction source.
 */
const IncomeTransactionSourceViewFrame = function ({
  account,
  location,
  incomeLines,
  incomeDeductions,
}: IncomeTransactionSourceViewFrameProps): JSX.Element {
  const grossAmount = incomeLines.reduce(
    (total, line) => total + line.amount,
    0,
  );
  const deductionAmount = incomeDeductions.reduce(
    (total, deduction) => total + deduction.amount,
    0,
  );
  const netAmount = grossAmount - deductionAmount;

  return (
    <TransactionFrame title="Income Source">
      {account !== null && <TransactionAccountViewDisplay account={account} />}
      {(location ?? "") !== "" && (
        <StringEntryField label="Source Location" value={location} />
      )}
      <Box
        sx={{
          display: "grid",
          gap: 1.5,
          gridTemplateColumns: {
            xs: "1fr",
            md: "repeat(3, minmax(0, 1fr))",
          },
        }}
      >
        <CurrencyEntryField label="Gross Income" value={grossAmount} />
        <CurrencyEntryField label="Deductions" value={deductionAmount} />
        <CurrencyEntryField label="Net Income" value={netAmount} />
      </Box>
      <IncomeTransactionItemSection
        title="Income Lines"
        description="Gross income amounts captured for this transaction."
        items={incomeLines}
      />
      <IncomeTransactionItemSection
        title="Income Deductions"
        description="Amounts withheld before the income was deposited."
        items={incomeDeductions}
      />
    </TransactionFrame>
  );
};

export default IncomeTransactionSourceViewFrame;
