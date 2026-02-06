import Caption from "@framework/dialog/Caption";
import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import type { FundAmount } from "@funds/ApiTypes";
import type { JSX } from "react/jsx-runtime";
import type { Transaction } from "@transactions/ApiTypes";
import { Typography } from "@mui/material";
import formatCurrency from "@framework/formatCurrency";

/**
 * Props for the TransactionFundFrame component.
 */
interface TransactionFundFrameProps {
  readonly transaction: Transaction;
}

/**
 * Interface representing a change in Transaction Fund Balance.
 */
interface TransactionFundBalanceChange {
  readonly previousBalance: FundAmount | null;
  readonly debitFundAmount: FundAmount | null;
  readonly isDebitPosted: boolean;
  readonly creditFundAmount: FundAmount | null;
  readonly isCreditPosted: boolean;
  readonly newBalance: FundAmount | null;
}

/**
 * Component that presents the user with a frame showing Transaction Fund details.
 * @param props - Props for the TransactionFundFrame component.
 * @returns JSX element representing the TransactionFundFrame.
 */
const TransactionFundFrame = function ({
  transaction,
}: TransactionFundFrameProps): JSX.Element {
  const getFundBalances = function (): TransactionFundBalanceChange[] {
    const balances: TransactionFundBalanceChange[] = [];
    const allFunds = (
      transaction.debitAccount?.fundAmounts.map((fa) => fa.fundId) ?? []
    ).concat(
      transaction.creditAccount?.fundAmounts.map((fa) => fa.fundId) ?? [],
    );
    const uniqueFunds = Array.from(new Set(allFunds));

    uniqueFunds.forEach((fund) => {
      const previousBalance =
        transaction.previousFundBalances.find((fb) => fb.fundId === fund)
          ?.balance ?? null;
      const debitFundAmount =
        transaction.debitAccount?.fundAmounts.find(
          (fa) => fa.fundId === fund,
        ) ?? null;
      const creditFundAmount =
        transaction.creditAccount?.fundAmounts.find(
          (fa) => fa.fundId === fund,
        ) ?? null;
      const newBalance =
        transaction.newFundBalances.find((fb) => fb.fundId === fund)?.balance ??
        null;
      if (debitFundAmount !== null || creditFundAmount !== null) {
        const fundId =
          debitFundAmount?.fundId ?? creditFundAmount?.fundId ?? "";
        const fundName =
          debitFundAmount?.fundName ?? creditFundAmount?.fundName ?? "";
        balances.push({
          previousBalance: { fundId, fundName, amount: previousBalance ?? 0 },
          debitFundAmount,
          isDebitPosted: transaction.debitAccount?.postedDate !== null,
          creditFundAmount,
          isCreditPosted: transaction.creditAccount?.postedDate !== null,
          newBalance: { fundId, fundName, amount: newBalance ?? 0 },
        });
      }
    });

    return balances;
  };

  return (
    <CaptionedFrame caption="Funds">
      {getFundBalances().map(
        ({
          previousBalance,
          debitFundAmount,
          isDebitPosted,
          creditFundAmount,
          isCreditPosted,
          newBalance,
        }) => (
          <div
            key={debitFundAmount?.fundName ?? creditFundAmount?.fundName ?? ""}
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr 1fr 1fr 1fr",
            }}
          >
            <Caption
              caption={
                debitFundAmount?.fundName ?? creditFundAmount?.fundName ?? ""
              }
            />
            <Typography variant="subtitle1">
              {formatCurrency(previousBalance?.amount ?? 0)}
            </Typography>
            <Typography
              variant="subtitle1"
              color={(debitFundAmount?.amount ?? 0) > 0 ? "red" : ""}
              align="center"
            >
              {`${isDebitPosted ? "" : "("} - ${formatCurrency(debitFundAmount?.amount ?? 0)} ${isDebitPosted ? "" : ")"}`}
            </Typography>
            <Typography
              variant="subtitle1"
              color={(creditFundAmount?.amount ?? 0) > 0 ? "green" : ""}
              align="center"
            >
              {`${isCreditPosted ? "" : "("} + ${formatCurrency(creditFundAmount?.amount ?? 0)} ${isCreditPosted ? "" : ")"}`}
            </Typography>
            <Typography variant="subtitle1" textAlign="right">
              {formatCurrency(newBalance?.amount ?? 0)}
            </Typography>
          </div>
        ),
      )}
    </CaptionedFrame>
  );
};

export default TransactionFundFrame;
