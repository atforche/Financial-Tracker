import {
  type AccountingPeriodAccount,
  isPositiveChangeInBalance,
} from "@/data/accountTypes";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import Stack from "@mui/material/Stack";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the AccountBalanceFrame component.
 */
interface AccountBalanceFrameProps {
  readonly account: AccountingPeriodAccount;
}

/**
 * Frame that displays the balance breakdown for an account in the context of an accounting period.
 */
const AccountBalanceFrame = function ({
  account,
}: AccountBalanceFrameProps): JSX.Element {
  const oldFundBalanceByFundName: Record<string, number> = {};
  const changeInFundBalanceByFundName: Record<string, number> = {};
  const newFundBalanceByFundName: Record<string, number> = {};

  const allFunds = new Set<string>();
  account.openingBalance.fundBalances.forEach((fundBalance) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allFunds.add(fundBalance.fundName),
  );
  account.closingBalance.fundBalances.forEach((fundBalance) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allFunds.add(fundBalance.fundName),
  );

  allFunds.forEach((fundName) => {
    const openingFundBalance = account.openingBalance.fundBalances.find(
      (fundBalance) => fundBalance.fundName === fundName,
    );
    const closingFundBalance = account.closingBalance.fundBalances.find(
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
      <CaptionedFrame caption="Opening Balance">
        <CaptionedValue
          caption="Total"
          value={formatCurrency(account.openingBalance.postedBalance)}
        />
        {Object.keys(oldFundBalanceByFundName).length > 0 ? (
          <>
            <br />
            {Object.entries(oldFundBalanceByFundName).map(
              ([accountName, amount]) => (
                <CaptionedValue
                  key={accountName}
                  caption={accountName}
                  value={formatCurrency(amount)}
                />
              ),
            )}
          </>
        ) : null}
      </CaptionedFrame>
      <CaptionedFrame caption="Change">
        <CaptionedValue
          caption="Total"
          value={
            isPositiveChangeInBalance(
              account.type,
              account.closingBalance.postedBalance -
                account.openingBalance.postedBalance,
            ) ? (
              <span style={{ color: "green" }}>
                +{" "}
                {formatCurrency(
                  Math.abs(
                    account.closingBalance.postedBalance -
                      account.openingBalance.postedBalance,
                  ),
                )}
              </span>
            ) : (
              <span style={{ color: "red" }}>
                -{" "}
                {formatCurrency(
                  Math.abs(
                    account.closingBalance.postedBalance -
                      account.openingBalance.postedBalance,
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
                    isPositiveChangeInBalance(account.type, amount) ? (
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
      <CaptionedFrame caption="Closing Balance">
        <CaptionedValue
          caption="Total"
          value={formatCurrency(account.closingBalance.postedBalance)}
        />
        {Object.keys(newFundBalanceByFundName).length > 0 ? (
          <>
            <br />
            {Object.entries(newFundBalanceByFundName).map(
              ([accountName, amount]) => (
                <CaptionedValue
                  key={accountName}
                  caption={accountName}
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

export default AccountBalanceFrame;
