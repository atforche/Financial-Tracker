import type { AccountBalanceSummary, AccountType } from "@/accounts/types";
import { formatAccountType, isTrackedAccountType } from "@/accounts/helpers";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { StatCardDetail } from "@/framework/view/StatCard";

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

/**
 * Gets the ending balance and change of each tracked or untracked account type, for a stat card's breakdown.
 */
const getAccountTypeStatDetails = function (
  startingBalance: AccountBalanceSummary,
  endingBalance: AccountBalanceSummary,
  tracked: boolean,
): StatCardDetail[] {
  return getAccountTypeBreakdownDetails(startingBalance, endingBalance)
    .filter(({ accountType }) => isTrackedAccountType(accountType) === tracked)
    .map((detail) => ({
      key: detail.accountType,
      label: formatAccountType(detail.accountType),
      value: formatCurrency(detail.endingBalance),
      change: detail.netChange,
    }));
};

export { getAccountTypeBreakdownDetails, getAccountTypeStatDetails };
export type { AccountTypeBreakdownDetail };
