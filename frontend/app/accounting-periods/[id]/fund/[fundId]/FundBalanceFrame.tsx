import type { AccountingPeriodFund } from "@/data/fundTypes";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import Stack from "@mui/material/Stack";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the FundBalanceFrame component.
 */
interface FundBalanceFrameProps {
  readonly fund: AccountingPeriodFund;
}

/**
 * Frame that displays the balance breakdown for a fund in the context of an accounting period.
 */
const FundBalanceFrame = function ({
  fund,
}: FundBalanceFrameProps): JSX.Element {
  const oldAccountBalanceByAccountName: Record<string, number> = {};
  const changeInAccountBalanceByAccountName: Record<string, number> = {};
  const newAccountBalanceByAccountName: Record<string, number> = {};

  const allAccounts = new Set<string>();
  fund.openingBalance.accountBalances.forEach((accountBalance) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allAccounts.add(accountBalance.accountName),
  );
  fund.closingBalance.accountBalances.forEach((accountBalance) =>
    // eslint-disable-next-line @typescript-eslint/strict-void-return
    allAccounts.add(accountBalance.accountName),
  );

  allAccounts.forEach((accountName) => {
    const openingAccountBalance = fund.openingBalance.accountBalances.find(
      (accountBalance) => accountBalance.accountName === accountName,
    );
    const closingAccountBalance = fund.closingBalance.accountBalances.find(
      (accountBalance) => accountBalance.accountName === accountName,
    );
    const openingAmount = openingAccountBalance?.amount ?? 0;
    const closingAmount = closingAccountBalance?.amount ?? 0;
    oldAccountBalanceByAccountName[accountName] = openingAmount;
    changeInAccountBalanceByAccountName[accountName] =
      closingAmount - openingAmount;
    newAccountBalanceByAccountName[accountName] = closingAmount;
  });

  return (
    <Stack spacing={2} direction="row">
      <CaptionedFrame caption="Opening Balance">
        <CaptionedValue
          caption="Total"
          value={formatCurrency(fund.openingBalance.postedBalance)}
        />
        {Object.keys(oldAccountBalanceByAccountName).length > 0 ? (
          <>
            <br />
            {Object.entries(oldAccountBalanceByAccountName).map(
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
            fund.closingBalance.postedBalance >
            fund.openingBalance.postedBalance ? (
              <span style={{ color: "green" }}>
                +{" "}
                {formatCurrency(
                  fund.closingBalance.postedBalance -
                    fund.openingBalance.postedBalance,
                )}
              </span>
            ) : (
              <span style={{ color: "red" }}>
                -{" "}
                {formatCurrency(
                  fund.closingBalance.postedBalance -
                    fund.openingBalance.postedBalance,
                )}
              </span>
            )
          }
        />
        {Object.keys(changeInAccountBalanceByAccountName).length > 0 ? (
          <>
            <br />
            {Object.entries(changeInAccountBalanceByAccountName).map(
              ([accountName, amount]) => (
                <CaptionedValue
                  key={accountName}
                  caption={accountName}
                  value={
                    amount > 0 ? (
                      <span style={{ color: "green" }}>
                        + {formatCurrency(amount)}
                      </span>
                    ) : (
                      <span style={{ color: "red" }}>
                        - {formatCurrency(amount)}
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
          value={formatCurrency(fund.closingBalance.postedBalance)}
        />
        {Object.keys(newAccountBalanceByAccountName).length > 0 ? (
          <>
            <br />
            {Object.entries(newAccountBalanceByAccountName).map(
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

export default FundBalanceFrame;
