import {
  AccountingPeriodSort,
  AccountingPeriodWithBalanceSort,
} from "@/accounting-periods/types";
import { Stack, Typography } from "@mui/material";
import AccountingPeriodCard from "@/accounting-periods/workspace/AccountingPeriodCard";
import type { AccountingPeriodWorkspaceAction } from "@/accounting-periods/workspace/helpers";
import AccountingPeriodWorkspaceActions from "@/accounting-periods/workspace/AccountingPeriodWorkspaceActions";
import AccountingPeriodYearSelector from "@/accounting-periods/workspace/AccountingPeriodYearSelector";
import CardResponsiveGrid from "@/framework/view/CardResponsiveGrid";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import PageLayout from "@/framework/view/PageLayout";
import ReturnLink from "@/framework/view/ReturnLink";
import createApiClient from "@/framework/data/createApiClient";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

/**
 * Search parameters supported by the accounting period workspace.
 */
interface AccountingPeriodWorkspaceSearchParams {
  year?: number | string;
  action?: AccountingPeriodWorkspaceAction;
  returnUrl?: string;
}

/**
 * Props for the AccountingPeriodWorkspace component.
 */
interface AccountingPeriodWorkspaceProps {
  readonly searchParams: Promise<AccountingPeriodWorkspaceSearchParams>;
}

/**
 * Displays one year of accounting periods as cards, newest first, with a selector for the other years that have
 * periods and the action to create the next period.
 */
const AccountingPeriodWorkspace = async function ({
  searchParams,
}: AccountingPeriodWorkspaceProps): Promise<JSX.Element> {
  const apiClient = await createApiClient();
  const { year, action, returnUrl } = await searchParams;
  const [firstResponse, latestResponse] = await Promise.all([
    apiClient.GET("/accounting-periods", {
      params: { query: { Sort: AccountingPeriodSort.Date, Limit: 1 } },
    }),
    apiClient.GET("/accounting-periods", {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 1 },
      },
    }),
  ]);
  const firstPeriod =
    unwrapApiResponse(
      firstResponse,
      "Failed to fetch the first accounting period",
    ).items[0] ?? null;
  const latestPeriod =
    unwrapApiResponse(
      latestResponse,
      "Failed to fetch the latest accounting period",
    ).items[0] ?? null;
  // Every year from the first period to the latest, newest first; an unknown year falls back to the latest
  const years =
    firstPeriod === null || latestPeriod === null
      ? []
      : Array.from(
          { length: latestPeriod.year - firstPeriod.year + 1 },
          (_, index) => latestPeriod.year - index,
        );
  const selectedYear =
    years.find((item) => item.toString() === year?.toString()) ?? years[0];
  const periods =
    selectedYear === undefined
      ? []
      : unwrapApiResponse(
          await apiClient.GET("/accounting-periods/with-balances", {
            params: {
              query: {
                "Filter.Years": [selectedYear],
                Sort: AccountingPeriodWithBalanceSort.DateDescending,
                Limit: 12,
              },
            },
          }),
          "Failed to fetch accounting periods",
        ).items;

  return (
    <PageLayout>
      <Stack spacing={2}>
        <ReturnLink returnUrl={returnUrl} />
        <Stack
          direction="row"
          useFlexGap
          spacing={2}
          sx={{
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h4">Accounting Periods</Typography>
          <AccountingPeriodWorkspaceActions
            latestAccountingPeriod={latestPeriod}
            requestedAction={action ?? null}
          />
        </Stack>
      </Stack>
      {selectedYear === undefined ? (
        <ContentSurface>
          <Stack spacing={0.5}>
            <Typography variant="subtitle2">
              No accounting periods yet
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Create the first accounting period to start tracking.
            </Typography>
          </Stack>
        </ContentSurface>
      ) : (
        <Stack spacing={2}>
          {years.length > 1 ? (
            <AccountingPeriodYearSelector value={selectedYear} years={years} />
          ) : null}
          <CardResponsiveGrid minimumColumnWidth={280} spacing={2}>
            {periods.map((period) => (
              <AccountingPeriodCard key={period.id} period={period} />
            ))}
          </CardResponsiveGrid>
        </Stack>
      )}
    </PageLayout>
  );
};

export type { AccountingPeriodWorkspaceSearchParams };
export default AccountingPeriodWorkspace;
