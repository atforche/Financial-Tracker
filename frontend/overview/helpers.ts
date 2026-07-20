import type {
  AccountOverviewSummary,
  FundOverviewSummary,
} from "@/overview/types";
import type { AccountWithBalance } from "@/accounts/types";
import type { FundWithBalance } from "@/funds/types";
import { isDebtAccountType } from "@/accounts/helpers";
import { isUnassignedFund } from "@/funds/helpers";

/**
 * Summarizes account balances for the overview page.
 */
const summarizeAccounts = function (
  accounts: readonly AccountWithBalance[],
): AccountOverviewSummary {
  const balances = accounts.map((account) => ({
    accountType: account.type,
    totalBalance:
      (isDebtAccountType(account.type) ? -1 : 1) *
      account.currentBalance.postedBalance,
  }));
  const balanceByAccountType = Array.from(
    Map.groupBy(balances, (balance) => balance.accountType),
    ([accountType, groupedBalances]) => ({
      accountType,
      totalBalance: groupedBalances.reduce(
        (total, balance) => total + balance.totalBalance,
        0,
      ),
    }),
  );

  return {
    totalBalance: balances.reduce(
      (total, balance) => total + balance.totalBalance,
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
