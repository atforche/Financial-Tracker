import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import { AccountingPeriodSort } from "@/accounting-periods/types";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { JSX } from "react";
import LocationCashFlowChart from "@/locations/LocationCashFlowChart";
import LocationTrendsFilter from "@/locations/LocationTrendsFilter";
import LocationTrendsTransactionListFrame from "@/locations/LocationTrendsTransactionListFrame";
import PageLayout from "@/framework/view/PageLayout";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import SummaryCard from "@/framework/view/SummaryCard";
import SummaryCardGrid from "@/framework/view/SummaryCardGrid";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { formatCurrency } from "@/framework/currencyHelpers";
import { redirect } from "next/navigation";
import { toRepeatedSearchParams } from "@/framework/routes/helpers";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface LocationTrendsSearchParams {
  readonly locationIds?: string | readonly string[];
  readonly mode?: "date" | "accounting-period";
  readonly startAccountingPeriodId?: string;
  readonly endAccountingPeriodId?: string;
  readonly startDate?: string;
  readonly endDate?: string;
  readonly page?: string | number;
  readonly pageSize?: string | number;
}

interface LocationTrendsProps {
  readonly searchParams: Promise<LocationTrendsSearchParams>;
}

/** Displays Location activity for a selected date or accounting-period range. */
const LocationTrends = async function ({
  searchParams,
}: LocationTrendsProps): Promise<JSX.Element> {
  const params = await searchParams;
  const defaultEndDate = dayjs();
  const defaultStartDate = defaultEndDate.subtract(90, "day");
  const startDate = params.startDate ?? defaultStartDate.format("YYYY-MM-DD");
  const endDate = params.endDate ?? defaultEndDate.format("YYYY-MM-DD");
  const currentMode =
    params.mode === "accounting-period" ? "accounting-period" : "date";
  const selectedLocationIds = [
    ...new Set(
      toRepeatedSearchParams(params.locationIds)
        .map((id) => id.trim())
        .filter((id) => id !== ""),
    ),
  ];
  const currentPage = normalizePageValue(params.page);
  const rowsPerPage = getRowsPerPage(params.pageSize);
  const apiClient = await createApiClient();
  const [locationsResponse, accountingPeriodsResponse] = await Promise.all([
    apiClient.GET("/locations", { params: { query: { Limit: 500 } } }),
    apiClient.GET("/accounting-periods", {
      params: {
        query: { Sort: AccountingPeriodSort.DateDescending, Limit: 500 },
      },
    }),
  ]);
  const locations = unwrapApiResponse(
    locationsResponse,
    "Failed to fetch Locations",
  );
  const accountingPeriods = unwrapApiResponse(
    accountingPeriodsResponse,
    "Failed to fetch accounting periods",
  );
  const latestAccountingPeriod = accountingPeriods.items[0] ?? null;

  if (currentMode === "accounting-period" && latestAccountingPeriod === null) {
    redirect("/locations/trends");
  }

  const startAccountingPeriodId =
    params.startAccountingPeriodId ?? latestAccountingPeriod?.id ?? "";
  const endAccountingPeriodId =
    params.endAccountingPeriodId ?? latestAccountingPeriod?.id ?? "";
  const query = {
    ...(selectedLocationIds.length > 0
      ? { "Filter.LocationIds": selectedLocationIds }
      : { "Filter.LocationIds": [] }),
    Limit: rowsPerPage,
    Offset: getPageOffset(currentPage, rowsPerPage),
  };
  const rangeResponse =
    currentMode === "date"
      ? await apiClient.GET("/transactions/date-range", {
          params: {
            query: { ...query, "Range.Start": startDate, "Range.End": endDate },
          },
        })
      : await apiClient.GET("/transactions/accounting-period-range", {
          params: {
            query: {
              ...query,
              "Range.Start": startAccountingPeriodId,
              "Range.End": endAccountingPeriodId,
            },
          },
        });
  const range = unwrapApiResponse(
    rangeResponse,
    "Failed to load Location trends",
  );
  const transactions =
    selectedLocationIds.length === 0 ? [] : range.transactions.items;
  const chartRange =
    selectedLocationIds.length === 0
      ? {
          transactions: { items: [] },
          locationIncomingAmount: 0,
          locationOutgoingAmount: 0,
        }
      : unwrapApiResponse(
          currentMode === "date"
            ? await apiClient.GET("/transactions/date-range", {
                params: {
                  query: {
                    "Filter.LocationIds": selectedLocationIds,
                    "Range.Start": startDate,
                    "Range.End": endDate,
                    Limit: 500,
                  },
                },
              })
            : await apiClient.GET("/transactions/accounting-period-range", {
                params: {
                  query: {
                    "Filter.LocationIds": selectedLocationIds,
                    "Range.Start": startAccountingPeriodId,
                    "Range.End": endAccountingPeriodId,
                    Limit: 500,
                  },
                },
              }),
          "Failed to load Location cash flow",
        );
  const incomingTotal = chartRange.locationIncomingAmount;
  const outgoingTotal = chartRange.locationOutgoingAmount;
  const transactionWorkspaceHref = transactionRoutes.workspace({
    locationIds: selectedLocationIds,
    ...(currentMode === "date"
      ? { startDate, endDate }
      : {
          accountingPeriodIds: accountingPeriods.items
            .slice(
              Math.min(
                accountingPeriods.items.findIndex(
                  (period) => period.id === startAccountingPeriodId,
                ),
                accountingPeriods.items.findIndex(
                  (period) => period.id === endAccountingPeriodId,
                ),
              ),
              Math.max(
                accountingPeriods.items.findIndex(
                  (period) => period.id === startAccountingPeriodId,
                ),
                accountingPeriods.items.findIndex(
                  (period) => period.id === endAccountingPeriodId,
                ),
              ) + 1,
            )
            .map((period) => period.id),
        }),
    returnUrl: "/locations/trends",
  });
  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="lg" />
      <ConstrainedContent>
        <LocationTrendsFilter
          accountingPeriods={accountingPeriods.items}
          locations={locations.items}
          defaultAccountingPeriodId={latestAccountingPeriod?.id ?? null}
          defaultStartDate={defaultStartDate.format("YYYY-MM-DD")}
          defaultEndDate={defaultEndDate.format("YYYY-MM-DD")}
        />
      </ConstrainedContent>
      <SummaryCardGrid>
        <SummaryCard
          title="Incoming Money"
          value={formatCurrency(incomingTotal)}
          description="Income received from the selected Locations."
        />
        <SummaryCard
          title="Outgoing Money"
          value={formatCurrency(outgoingTotal)}
          description="Spending paid to the selected Locations."
        />
      </SummaryCardGrid>
      <LocationCashFlowChart
        transactions={chartRange.transactions.items}
        mode={currentMode === "date" ? "Date" : "AccountingPeriod"}
        {...(currentMode === "date" ? { startDate, endDate } : {})}
      />
      <LocationTrendsTransactionListFrame
        transactions={transactions}
        totalCount={
          selectedLocationIds.length === 0 ? 0 : range.transactions.totalCount
        }
        hasSelectedLocations={selectedLocationIds.length > 0}
        transactionWorkspaceHref={transactionWorkspaceHref}
      />
    </PageLayout>
  );
};

export default LocationTrends;
