import type { JSX, ReactNode } from "react";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountBalanceSummary } from "@/accounts/types";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";
import { getAccountTypeStatDetails } from "@/accounts/balanceSummaryHelpers";

/**
 * Props for the BalanceStatCards component.
 */
interface BalanceStatCardsProps {
  readonly openingBalance: AccountBalanceSummary;
  readonly closingBalance: AccountBalanceSummary;
  readonly unassignedBalance: number;
  readonly unassignedCaption: ReactNode;
  readonly changeDescription: string;
}

/**
 * Displays net worth, tracked money, and untracked money with how each changed between two balances, along with the
 * unassigned fund balance.
 */
const BalanceStatCards = function ({
  openingBalance,
  closingBalance,
  unassignedBalance,
  unassignedCaption,
  changeDescription,
}: BalanceStatCardsProps): JSX.Element {
  const describeChange = function (
    current: number,
    previous: number,
  ): JSX.Element {
    return (
      <>
        <SignedChange amount={getCurrencyDifference(current, previous)} />{" "}
        {changeDescription}
      </>
    );
  };
  return (
    <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }}>
      <StatCard
        label="Net worth"
        value={formatCurrency(closingBalance.totalBalance)}
        caption={describeChange(
          closingBalance.totalBalance,
          openingBalance.totalBalance,
        )}
      />
      <StatCard
        label="Tracked money"
        value={formatCurrency(closingBalance.totalTrackedBalance)}
        caption={describeChange(
          closingBalance.totalTrackedBalance,
          openingBalance.totalTrackedBalance,
        )}
        details={getAccountTypeStatDetails(
          openingBalance,
          closingBalance,
          true,
        )}
        detailsLabel="by account type"
      />
      <StatCard
        label="Untracked money"
        value={formatCurrency(closingBalance.totalUntrackedBalance)}
        caption={describeChange(
          closingBalance.totalUntrackedBalance,
          openingBalance.totalUntrackedBalance,
        )}
        details={getAccountTypeStatDetails(
          openingBalance,
          closingBalance,
          false,
        )}
        detailsLabel="by account type"
      />
      <StatCard
        label="Unassigned funds"
        value={formatCurrency(unassignedBalance)}
        caption={unassignedCaption}
      />
    </ResponsiveGrid>
  );
};

export default BalanceStatCards;
