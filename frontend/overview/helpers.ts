import type {
  AccountOverviewSummary,
  FundOverviewSummary,
} from "@/overview/types";
import type { AccountWithBalance } from "@/accounts/types";
import type { FundWithBalance } from "@/funds/types";
import { isUnassignedFund } from "@/funds/helpers";

/**
 * Summarizes account balances for the overview page.
 */
const summarizeAccounts = function (
  accounts: readonly AccountWithBalance[],
): AccountOverviewSummary {
  const balanceByAccountType = Array.from(
    Map.groupBy(accounts, (account) => account.type),
    ([accountType, groupedAccounts]) => ({
      accountType,
      totalBalance: groupedAccounts.reduce(
        (total, account) => total + account.currentBalance.postedBalance,
        0,
      ),
    }),
  );

  return {
    totalBalance: accounts.reduce(
      (total, account) => total + account.currentBalance.postedBalance,
      0,
    ),
    balanceByAccountType,
  };
};

/**
 * Summarizes fund balances for the overview page.
 */
const summarizeFunds = function (
  funds: readonly FundWithBalance[],
): FundOverviewSummary {
  const unassignedFund = funds.find((fund) => isUnassignedFund(fund.name));

  return {
    totalBalance: funds.reduce(
      (total, fund) => total + fund.currentBalance.postedBalance,
      0,
    ),
    totalAssignedBalance: funds
      .filter((fund) => !isUnassignedFund(fund.name))
      .reduce((total, fund) => total + fund.currentBalance.postedBalance, 0),
    totalUnassignedBalance: unassignedFund?.currentBalance.postedBalance ?? 0,
  };
};

export { summarizeAccounts, summarizeFunds };
