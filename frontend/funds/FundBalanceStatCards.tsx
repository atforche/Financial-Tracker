import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { FundBalanceSummary } from "@/funds/types";
import type { JSX } from "react";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import SignedChange from "@/framework/view/SignedChange";
import StatCard from "@/framework/view/StatCard";

/**
 * Props for the FundBalanceStatCards component.
 */
interface FundBalanceStatCardsProps {
  readonly startingBalance: FundBalanceSummary;
  readonly endingBalance: FundBalanceSummary;
  readonly changeDescription: string;
}

/**
 * Displays the total, assigned, and unassigned fund balances with how each changed over a range.
 */
const FundBalanceStatCards = function ({
  startingBalance,
  endingBalance,
  changeDescription,
}: FundBalanceStatCardsProps): JSX.Element {
  const cards = [
    { label: "Total balance", key: "totalBalance" },
    { label: "Assigned", key: "totalAssignedBalance" },
    { label: "Unassigned", key: "totalUnassignedBalance" },
  ] as const;
  return (
    <ResponsiveGrid columns={{ xs: 2, sm: 3 }} uniformPhoneRows>
      {cards.map(({ label, key }) => (
        <StatCard
          key={key}
          label={label}
          value={formatCurrency(endingBalance[key])}
          caption={
            <>
              <SignedChange
                amount={getCurrencyDifference(
                  endingBalance[key],
                  startingBalance[key],
                )}
              />{" "}
              {changeDescription}
            </>
          }
        />
      ))}
    </ResponsiveGrid>
  );
};

export default FundBalanceStatCards;
