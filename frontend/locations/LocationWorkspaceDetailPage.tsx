import { Button, Stack, Typography } from "@mui/material";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/transactionListHelpers";
import routes, { type LocationDetailSearchParams } from "@/locations/routes";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import LocationDetailFilter from "@/locations/LocationDetailFilter";
import LocationMonthlyChart from "@/locations/LocationMonthlyChart";
import LocationPaymentBreakdown from "@/locations/LocationPaymentBreakdown";
import PageLayout from "@/framework/view/PageLayout";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import RowList from "@/framework/view/RowList";
import StatCard from "@/framework/view/StatCard";
import createApiClient from "@/framework/data/createApiClient";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatShortDate } from "@/framework/dateHelpers";
import isEntityId from "@/framework/data/isEntityId";
import { redirect } from "next/navigation";
import resolveLocationRange from "@/locations/resolveLocationRange";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

const recentTransactionCount = 10;

/**
 * Shows one location over a date or period range: its totals, spending by month, the accounts and funds that paid
 * it, and its most recent transactions.
 */
const LocationWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: {
  readonly params: Promise<{ readonly locationId: string }>;
  readonly searchParams: Promise<LocationDetailSearchParams>;
}): Promise<JSX.Element> {
  const { locationId } = await params;
  const resolvedSearchParams = await searchParams;
  const { returnUrl } = resolvedSearchParams;
  const workspaceUrl = routes.workspace({});
  if (!isEntityId(locationId)) {
    redirect(workspaceUrl);
  }
  const { accountingPeriods, range } = await resolveLocationRange(
    resolvedSearchParams,
    (canonicalRange) =>
      routes.workspaceDetail(locationId, {
        ...(returnUrl === undefined ? {} : { returnUrl }),
        ...canonicalRange,
      }),
  );
  const apiClient = await createApiClient();
  const path = { locationId };
  const [detailResponse, transactionsResponse] = await Promise.all([
    range.mode === "date"
      ? apiClient.GET("/locations/{locationId}/date-range", {
          params: {
            path,
            query: {
              "Range.Start": range.startDate,
              "Range.End": range.endDate,
            },
          },
        })
      : apiClient.GET("/locations/{locationId}/accounting-period-range", {
          params: {
            path,
            query: {
              "Range.Start": range.startAccountingPeriodId,
              "Range.End": range.endAccountingPeriodId,
            },
          },
        }),
    apiClient.GET("/transactions", {
      params: {
        query: {
          "Filter.LocationIds": [locationId],
          Limit: recentTransactionCount,
        },
      },
    }),
  ]);
  if (detailResponse.response.status === 404) {
    redirect(workspaceUrl);
  }
  const { summary, months, accounts, funds } = unwrapApiResponse(
    detailResponse,
    "Failed to fetch location activity",
  );
  const transactions = unwrapApiResponse(
    transactionsResponse,
    "Failed to fetch location transactions",
  );
  const currentUrl = routes.workspaceDetail(locationId, {
    ...(returnUrl === undefined ? {} : { returnUrl }),
    ...range,
  });

  return (
    <PageLayout>
      <LocationDetailFilter
        location={{ id: summary.id, name: summary.name }}
        accountingPeriods={accountingPeriods}
        returnUrl={returnUrl ?? workspaceUrl}
        workspaceUrl={workspaceUrl}
      />
      <ResponsiveGrid columns={{ xs: 1, sm: 2, lg: 4 }}>
        <StatCard
          label="Spent"
          value={formatCurrency(summary.spent)}
          caption="Paid here in this range"
        />
        <StatCard
          label="Received"
          value={formatCurrency(summary.received)}
          caption="Income and refunds"
        />
        <StatCard
          label="Transactions"
          value={summary.transactionCount.toString()}
          caption={
            summary.transactionCount === 0
              ? "None in this range"
              : `${formatCurrency(summary.spent / summary.transactionCount)} average spent`
          }
        />
        <StatCard
          label="Last activity"
          value={
            summary.lastActivity === null || summary.lastActivity === undefined
              ? "—"
              : formatShortDate(new Date(`${summary.lastActivity}T00:00:00`))
          }
          caption="Latest transaction in this range"
        />
      </ResponsiveGrid>
      <LocationMonthlyChart months={months} />
      <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
        <LocationPaymentBreakdown
          title="Paid From"
          shares={accounts}
          emptyMessage="Nothing was paid here from an account in this range."
        />
        <LocationPaymentBreakdown
          title="Paid With"
          shares={funds}
          emptyMessage="No fund paid for anything here in this range."
        />
      </ResponsiveGrid>
      <ContentSurface>
        <Stack spacing={1.5}>
          <Stack
            direction="row"
            spacing={2}
            sx={{ alignItems: "center", justifyContent: "space-between" }}
          >
            <Typography variant="h5">Recent Transactions</Typography>
            {transactions.totalCount > recentTransactionCount ? (
              <Button
                component={Link}
                href={transactionRoutes.workspace({
                  locationIds: [locationId],
                  returnUrl: currentUrl,
                })}
                variant="outlined"
              >
                View all {transactions.totalCount}
              </Button>
            ) : null}
          </Stack>
          {transactions.items.length === 0 ? (
            <Typography variant="body2" color="text.secondary">
              No transactions name this location.
            </Typography>
          ) : (
            <RowList
              rows={transactions.items.map((transaction) => ({
                key: transaction.id,
                href: transactionRoutes.workspaceDetail(transaction.id, {
                  returnUrl: currentUrl,
                }),
                leading: formatShortDate(
                  new Date(`${transaction.date}T00:00:00`),
                ),
                title: transaction.description,
                detail: `${getTransactionSourceLabel(transaction)} → ${getTransactionDestinationLabel(transaction)}`,
                value: formatCurrency(transaction.amount),
              }))}
            />
          )}
        </Stack>
      </ContentSurface>
    </PageLayout>
  );
};

export default LocationWorkspaceDetailPage;
