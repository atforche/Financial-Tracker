import {
  type TransactionAccount,
  TransactionAccountType,
} from "@transactions/ApiTypes";
import Caption from "@framework/dialog/Caption";
import type { FundAmount } from "@funds/ApiTypes";
import type { JSX } from "react";
import { Typography } from "@mui/material";
import formatCurrency from "@framework/formatCurrency";

/**
 * Props for the TransactionAccountBalanceFrame component.
 */
interface TransactionAccountBalanceFrameProps {
  readonly transactionAccount: TransactionAccount;
  readonly transactionAccountType: TransactionAccountType;
}

/**
 * Interface representing a change in Transaction Account Balance.
 */
interface TransactionAccountBalanceChange {
  readonly previousBalance: FundAmount | null;
  readonly change: FundAmount | null;
  readonly isPosted: boolean;
  readonly newBalance: FundAmount | null;
}

/**
 * Component that presents the user with a frame showing Transaction Account Balance details.
 * @param props - Props for the TransactionAccountBalanceFrame component.
 * @returns JSX element representing the TransactionAccountBalanceFrame.
 */
const TransactionAccountBalanceFrame = function ({
  transactionAccount,
  transactionAccountType,
}: TransactionAccountBalanceFrameProps): JSX.Element {
  const getAccountBalances = function (): TransactionAccountBalanceChange[] {
    const balances: TransactionAccountBalanceChange[] = [];
    const allFundIds = transactionAccount.previousAccountBalance.fundBalances
      .map((fb) => fb.fundId)
      .concat(transactionAccount.fundAmounts.map((fa) => fa.fundId))
      .concat(
        transactionAccount.newAccountBalance.fundBalances.map(
          (fb) => fb.fundId,
        ),
      );
    const uniqueFundIds = Array.from(new Set(allFundIds));

    uniqueFundIds.forEach((fundId) => {
      const previousBalance =
        transactionAccount.previousAccountBalance.fundBalances.find(
          (fb) => fb.fundId === fundId,
        ) ?? null;
      const fundAmount =
        transactionAccount.fundAmounts.find((fa) => fa.fundId === fundId) ??
        null;
      const newBalance =
        transactionAccount.newAccountBalance.fundBalances.find(
          (fb) => fb.fundId === fundId,
        ) ?? null;
      if (previousBalance || fundAmount || newBalance) {
        balances.push({
          previousBalance,
          change: fundAmount,
          isPosted: transactionAccount.postedDate !== null,
          newBalance,
        });
      }
    });

    return balances;
  };

  return (
    <>
      {getAccountBalances().map(
        ({ previousBalance, change, isPosted, newBalance }) => (
          <div
            key={
              previousBalance?.fundName ??
              change?.fundName ??
              newBalance?.fundName
            }
            style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 1fr" }}
          >
            <Typography variant="subtitle1">
              {`— ${previousBalance?.fundName ?? change?.fundName ?? newBalance?.fundName}`}
            </Typography>
            <Typography variant="subtitle1">
              {formatCurrency(previousBalance?.amount ?? 0)}
            </Typography>
            <Typography
              variant="subtitle1"
              color={
                transactionAccountType === TransactionAccountType.Credit
                  ? "green"
                  : "red"
              }
              sx={{ textAlign: "center" }}
            >
              {`${isPosted ? "" : "("}  ${transactionAccountType === TransactionAccountType.Credit ? "+" : "-"} ${formatCurrency(change?.amount ?? 0)} ${isPosted ? "" : ")"} `}
            </Typography>
            <Typography variant="subtitle1" sx={{ textAlign: "right" }}>
              {formatCurrency(newBalance?.amount ?? 0)}
            </Typography>
          </div>
        ),
      )}
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 1fr" }}>
        <Caption caption="Total" />
        <Typography variant="subtitle1">
          {formatCurrency(transactionAccount.previousAccountBalance.balance)}
        </Typography>
        <Typography
          variant="subtitle1"
          color={
            transactionAccountType === TransactionAccountType.Credit
              ? "green"
              : "red"
          }
          sx={{ textAlign: "center" }}
        >
          {`${transactionAccount.postedDate === null ? "(" : ""} ${transactionAccountType === TransactionAccountType.Credit ? "+" : "-"} ${formatCurrency(transactionAccount.fundAmounts.reduce((sum, fundAmount) => sum + fundAmount.amount, 0))} ${transactionAccount.postedDate === null ? ")" : ""}`}
        </Typography>
        <Typography variant="subtitle1" sx={{ textAlign: "right" }}>
          {formatCurrency(transactionAccount.newAccountBalance.balance)}
        </Typography>
      </div>
    </>
  );
};

export default TransactionAccountBalanceFrame;
