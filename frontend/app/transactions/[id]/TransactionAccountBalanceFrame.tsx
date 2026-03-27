import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import type { TransactionAccount } from "@/data/transactionTypes";
import formatCurrency from "@/framework/formatCurrency";
import { isPositiveChangeInBalance } from "@/data/accountTypes";

/**
 * Props for the TransactionAccountBalanceFrame component.
 */
interface TransactionAccountBalanceFrameProps {
  readonly transactionAccount: TransactionAccount;
}

/**
 * Component that presents the user with a frame showing Transaction Account Balance details.
 */
const TransactionAccountBalanceFrame = function ({
  transactionAccount,
}: TransactionAccountBalanceFrameProps): JSX.Element {
  const oldFundBalanceByFundName: Record<string, number> = {};
  const changeInFundBalanceByFundName: Record<string, number> = {};
  const newFundBalanceByFundName: Record<string, number> = {};

  const allFunds = new Set<string>();
  transactionAccount.previousAccountBalance.fundBalances.forEach(
    (fundBalance) =>
      // eslint-disable-next-line @typescript-eslint/strict-void-return
      allFunds.add(fundBalance.fundName),
  );
  transactionAccount.newAccountBalance.fundBalances.forEach((fundBalance) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allFunds.add(fundBalance.fundName),
  );

  allFunds.forEach((fundName) => {
    const openingFundBalance =
      transactionAccount.previousAccountBalance.fundBalances.find(
        (fundBalance) => fundBalance.fundName === fundName,
      );
    const closingFundBalance =
      transactionAccount.newAccountBalance.fundBalances.find(
        (fundBalance) => fundBalance.fundName === fundName,
      );
    const openingAmount = openingFundBalance?.amount ?? 0;
    const closingAmount = closingFundBalance?.amount ?? 0;
    oldFundBalanceByFundName[fundName] = openingAmount;
    changeInFundBalanceByFundName[fundName] = closingAmount - openingAmount;
    newFundBalanceByFundName[fundName] = closingAmount;
  });

  return (
    <Stack spacing={2} direction="row">
      <CaptionedFrame caption="Previous Balance" minWidth={200}>
        <CaptionedValue
          caption="Total"
          value={formatCurrency(
            transactionAccount.previousAccountBalance.postedBalance,
          )}
        />
        {Object.keys(oldFundBalanceByFundName).length > 0 ? (
          <>
            <br />
            {Object.entries(oldFundBalanceByFundName).map(
              ([fundName, amount]) => (
                <CaptionedValue
                  key={fundName}
                  caption={fundName}
                  value={formatCurrency(amount)}
                />
              ),
            )}
          </>
        ) : null}
      </CaptionedFrame>
      <CaptionedFrame caption="Change" minWidth={200}>
        <CaptionedValue
          caption="Total"
          value={
            isPositiveChangeInBalance(
              transactionAccount.accountType,
              transactionAccount.newAccountBalance.postedBalance -
                transactionAccount.previousAccountBalance.postedBalance,
            ) ? (
              <span style={{ color: "green" }}>
                +{" "}
                {formatCurrency(
                  Math.abs(
                    transactionAccount.newAccountBalance.postedBalance -
                      transactionAccount.previousAccountBalance.postedBalance,
                  ),
                )}
              </span>
            ) : (
              <span style={{ color: "red" }}>
                -{" "}
                {formatCurrency(
                  Math.abs(
                    transactionAccount.newAccountBalance.postedBalance -
                      transactionAccount.previousAccountBalance.postedBalance,
                  ),
                )}
              </span>
            )
          }
        />
        {Object.keys(changeInFundBalanceByFundName).length > 0 ? (
          <>
            <br />
            {Object.entries(changeInFundBalanceByFundName).map(
              ([accountName, amount]) => (
                <CaptionedValue
                  key={accountName}
                  caption={accountName}
                  value={
                    isPositiveChangeInBalance(
                      transactionAccount.accountType,
                      amount,
                    ) ? (
                      <span style={{ color: "green" }}>
                        + {formatCurrency(Math.abs(amount))}
                      </span>
                    ) : (
                      <span style={{ color: "red" }}>
                        - {formatCurrency(Math.abs(amount))}
                      </span>
                    )
                  }
                />
              ),
            )}
          </>
        ) : null}
      </CaptionedFrame>
      <CaptionedFrame caption="New Balance" minWidth={200}>
        <CaptionedValue
          caption="Total"
          value={formatCurrency(
            transactionAccount.newAccountBalance.postedBalance,
          )}
        />
        {Object.keys(newFundBalanceByFundName).length > 0 ? (
          <>
            <br />
            {Object.entries(newFundBalanceByFundName).map(
              ([fundName, amount]) => (
                <CaptionedValue
                  key={fundName}
                  caption={fundName}
                  value={formatCurrency(amount)}
                />
              ),
            )}
          </>
        ) : null}
      </CaptionedFrame>
    </Stack>
  );
};

export default TransactionAccountBalanceFrame;
