import type { AccountBalanceSummary, AccountType } from "@/accounts/types";
import { getCurrencyDifference } from "@/framework/currencyHelpers";

const emptyAccountBalanceSummary: AccountBalanceSummary = {
  totalBalance: 0,
  totalTrackedBalance: 0,
  totalUntrackedBalance: 0,
  balanceByAccountType: [],
};

/**
 * Details of account balances and changes for a specific account type.
 */
interface AccountTypeBreakdownDetail {
  readonly accountType: AccountType;
  readonly startingBalance: number;
  readonly endingBalance: number;
  readonly netChange: number;
}

/**
 * Merges starting and ending balances into account-type change details.
 */
const getAccountTypeBreakdownDetails = function (
  startingBalance: AccountBalanceSummary,
  endingBalance: AccountBalanceSummary,
): AccountTypeBreakdownDetail[] {
  const starting = new Map(
    startingBalance.balanceByAccountType.map(
      ({ accountType, totalBalance }) => [accountType, totalBalance],
    ),
  );
  const ending = new Map(
    endingBalance.balanceByAccountType.map(({ accountType, totalBalance }) => [
      accountType,
      totalBalance,
    ]),
  );

  return Array.from(new Set([...starting.keys(), ...ending.keys()]))
    .map((accountType) => {
      const startingAmount = starting.get(accountType) ?? 0;
      const endingAmount = ending.get(accountType) ?? 0;
      return {
        accountType,
        startingBalance: startingAmount,
        endingBalance: endingAmount,
        netChange: getCurrencyDifference(endingAmount, startingAmount),
      };
    })
    .sort((left, right) =>
      getCurrencyDifference(
        Math.abs(right.endingBalance),
        Math.abs(left.endingBalance),
      ),
    );
};

export { emptyAccountBalanceSummary, getAccountTypeBreakdownDetails };
export type { AccountTypeBreakdownDetail };
