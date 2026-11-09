import {
  compareCurrencyAmounts,
  formatCurrency,
} from "@/framework/currencyHelpers";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import EntityCard from "@/framework/view/EntityCard";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import WorkspaceEmptyState from "@/framework/view/WorkspaceEmptyState";
import routes from "@/funds/routes";

/**
 * Props for the FundWorkspaceCards component.
 */
interface FundWorkspaceCardsProps {
  readonly data: FundWithBalance[];
  readonly search: string | undefined;
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays the workspace's funds as linked cards.
 */
const FundWorkspaceCards = function ({
  data,
  search,
  isInOnboardingMode,
}: FundWorkspaceCardsProps): JSX.Element {
  if (data.length === 0) {
    return (search ?? "").trim() === "" ? (
      <WorkspaceEmptyState
        title="No funds yet"
        message={
          isInOnboardingMode
            ? "Use onboarding to add the first fund to your workspace."
            : "Create a fund to start building your workspace."
        }
      />
    ) : (
      <WorkspaceEmptyState
        title="No matching funds"
        message="Try a different name."
      />
    );
  }

  return (
    <CardResponsiveGrid minimumColumnWidth={280} spacing={2}>
      {data.map((fund) => (
        <EntityCard
          key={fund.id}
          name={fund.name}
          href={routes.workspaceDetail(
            fund.id,
            search === undefined ? {} : { search },
          )}
          color={
            compareCurrencyAmounts(fund.currentBalance.postedBalance, 0) >= 0
              ? "info"
              : "error"
          }
          balance={formatCurrency(fund.currentBalance.postedBalance)}
        />
      ))}
    </CardResponsiveGrid>
  );
};

export default FundWorkspaceCards;
