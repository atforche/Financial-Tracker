import type { AccountWithBalance } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import EntityCard from "@/framework/view/EntityCard";
import InstitutionSection from "@/accounts/workspace/InstitutionSection";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import WorkspaceEmptyState from "@/framework/view/WorkspaceEmptyState";
import { formatCurrency } from "@/framework/currencyHelpers";
import { getAccountCardColor } from "@/accounts/workspace/helpers";
import routes from "@/accounts/routes";

/**
 * Props for the AccountWorkspaceCards component.
 */
interface AccountWorkspaceCardsProps {
  readonly data: AccountWithBalance[];
  readonly filters: Pick<
    AccountWorkspaceSearchParams,
    "search" | "accountType"
  >;
  readonly hasActiveFilters: boolean;
  readonly isInOnboardingMode: boolean;
}

const noFinancialInstitutionLabel = "No Financial Institution";

/**
 * Compares two strings for sorting.
 */
const compareStrings = function (left: string, right: string): number {
  return left < right ? -1 : left > right ? 1 : 0;
};

/**
 * Displays the workspace's accounts as linked cards, grouped by financial institution.
 */
const AccountWorkspaceCards = function ({
  data,
  filters,
  hasActiveFilters,
  isInOnboardingMode,
}: AccountWorkspaceCardsProps): JSX.Element {
  if (data.length === 0) {
    return hasActiveFilters ? (
      <WorkspaceEmptyState
        title="No matching accounts"
        message="Try a different search or account type."
      />
    ) : (
      <WorkspaceEmptyState
        title="No accounts yet"
        message={
          isInOnboardingMode
            ? "Use onboarding to add the first account to your workspace."
            : "Create an account to start building your workspace."
        }
      />
    );
  }

  const groups = Map.groupBy(
    data.toSorted((left, right) => compareStrings(left.name, right.name)),
    (account) => account.financialInstitution ?? noFinancialInstitutionLabel,
  );
  return (
    <Stack spacing={2}>
      {[...groups.entries()]
        .sort(([left], [right]) => compareStrings(left, right))
        .map(([financialInstitution, accounts]) => (
          <InstitutionSection
            key={financialInstitution}
            institution={financialInstitution}
            accountCount={accounts.length}
          >
            <CardResponsiveGrid minimumColumnWidth={280} spacing={2}>
              {accounts.map((account) => (
                <EntityCard
                  key={account.id}
                  name={account.name}
                  href={routes.workspaceDetail(account.id, {
                    ...filters,
                    institution: financialInstitution,
                  })}
                  color={getAccountCardColor(account.type)}
                  balance={formatCurrency(account.currentBalance.postedBalance)}
                />
              ))}
            </CardResponsiveGrid>
          </InstitutionSection>
        ))}
    </Stack>
  );
};

export default AccountWorkspaceCards;
