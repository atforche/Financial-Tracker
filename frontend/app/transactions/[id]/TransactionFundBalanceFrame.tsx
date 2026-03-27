import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import type { Transaction } from "@/data/transactionTypes";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the TransactionFundBalanceFrame component.
 */
interface TransactionFundBalanceFrameProps {
  readonly transaction: Transaction;
}

/**
 * Component that presents the user with a frame showing Transaction Fund Balance details.
 */
const TransactionFundBalanceFrame = function ({
  transaction,
}: TransactionFundBalanceFrameProps): JSX.Element {
  const oldFundBalanceByFundName: Record<string, number> = {};
  const debitFundAmountByFundName: Record<string, number> = {};
  const creditFundAmountByFundName: Record<string, number> = {};
  const newFundBalanceByFundName: Record<string, number> = {};

  const allFunds = new Set<string>();
  transaction.debitAccount?.fundAmounts.forEach((fundAmount) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allFunds.add(fundAmount.fundName),
  );
  transaction.creditAccount?.fundAmounts.forEach((fundAmount) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allFunds.add(fundAmount.fundName),
  );

  allFunds.forEach((fundName) => {
    const debitFundAmount = transaction.debitAccount?.fundAmounts.find(
      (fundAmount) => fundAmount.fundName === fundName,
    );
    const creditFundAmount = transaction.creditAccount?.fundAmounts.find(
      (fundAmount) => fundAmount.fundName === fundName,
    );
    const debitAmount = debitFundAmount?.amount ?? 0;
    const creditAmount = creditFundAmount?.amount ?? 0;
    debitFundAmountByFundName[fundName] = debitAmount;
    creditFundAmountByFundName[fundName] = creditAmount;
    oldFundBalanceByFundName[fundName] =
      transaction.previousFundBalances.find(
        (fundBalance) => fundBalance.fundName === fundName,
      )?.postedBalance ?? 0;
    newFundBalanceByFundName[fundName] =
      transaction.newFundBalances.find(
        (fundBalance) => fundBalance.fundName === fundName,
      )?.postedBalance ?? 0;
  });

  return (
    <CaptionedFrame caption="Funds" minWidth={500} maxWidth={null}>
      <Stack spacing={2} direction="row">
        <CaptionedFrame caption="Previous Fund Balances" minWidth={200}>
          {Object.entries(oldFundBalanceByFundName).map(
            ([fundName, balance]) => (
              <CaptionedValue
                key={fundName}
                caption={fundName}
                value={formatCurrency(balance)}
              />
            ),
          )}
        </CaptionedFrame>
        <CaptionedFrame caption="Debit Amount" minWidth={200}>
          {Object.entries(debitFundAmountByFundName).map(
            ([fundName, amount]) => (
              <CaptionedValue
                key={fundName}
                caption={fundName}
                value={
                  <span style={{ color: "red" }}>{formatCurrency(amount)}</span>
                }
              />
            ),
          )}
        </CaptionedFrame>
        <CaptionedFrame caption="Credit Amount" minWidth={200}>
          {Object.entries(creditFundAmountByFundName).map(
            ([fundName, amount]) => (
              <CaptionedValue
                key={fundName}
                caption={fundName}
                value={
                  <span style={{ color: "green" }}>
                    {formatCurrency(amount)}
                  </span>
                }
              />
            ),
          )}
        </CaptionedFrame>
        <CaptionedFrame caption="New Fund Balances" minWidth={200}>
          {Object.entries(newFundBalanceByFundName).map(
            ([fundName, balance]) => (
              <CaptionedValue
                key={fundName}
                caption={fundName}
                value={formatCurrency(balance)}
              />
            ),
          )}
        </CaptionedFrame>
      </Stack>
    </CaptionedFrame>
  );
};

export default TransactionFundBalanceFrame;
