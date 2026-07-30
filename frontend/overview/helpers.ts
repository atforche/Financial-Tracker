import type {
  AccountOverviewSummary,
  FundOverviewSummary,
} from "@/overview/types";
import type { AccountWithBalance } from "@/accounts/types";
import type { FundWithBalance } from "@/funds/types";
import { getCurrencyTotal } from "@/framework/currencyHelpers";
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
      totalBalance: getCurrencyTotal(
        groupedBalances.map((balance) => balance.totalBalance),
      ),
    }),
  );

  return {
    totalBalance: getCurrencyTotal(
      balances.map((balance) => balance.totalBalance),
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
    totalBalance: getCurrencyTotal(
      funds.map((fund) => fund.currentBalance.postedBalance),
    ),
    totalAssignedBalance: getCurrencyTotal(
      funds
        .filter((fund) => !isUnassignedFund(fund.name))
        .map((fund) => fund.currentBalance.postedBalance),
    ),
    totalUnassignedBalance: unassignedFund?.currentBalance.postedBalance ?? 0,
  };
};

export { summarizeAccounts, summarizeFunds };
