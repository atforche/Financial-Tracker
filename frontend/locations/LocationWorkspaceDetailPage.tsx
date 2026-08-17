import { Box, Button, Stack, Typography } from "@mui/material";
import {
  getPageOffset,
  getRowsPerPage,
  normalizePageValue,
} from "@/framework/listframe/page";
import ArrowBack from "@mui/icons-material/ArrowBack";
import type { JSX } from "react";
import Link from "next/link";
import LocationCashFlowChart from "@/locations/LocationCashFlowChart";
import LocationDetailActions from "@/locations/LocationDetailActions";
import type { LocationWorkspaceSearchParams } from "@/locations/LocationWorkspace";
import PageLayout from "@/framework/view/PageLayout";
import RecentLocationTransactionsFrame from "@/locations/RecentLocationTransactionsFrame";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import ResponsivePageSize from "@/framework/listframe/ResponsivePageSize";
import SummaryCard from "@/framework/view/SummaryCard";
import createApiClient from "@/framework/data/createApiClient";
import dayjs from "dayjs";
import { redirect } from "next/navigation";
import routes from "@/locations/routes";
import transactionRoutes from "@/transactions/routes";
import unwrapApiResponse from "@/framework/data/unwrapApiResponse";

interface LocationWorkspaceDetailPageProps {
  readonly params: Promise<{ locationId: string }>;
  readonly searchParams: Promise<LocationWorkspaceSearchParams>;
}

/** Displays one Location and its recent transaction activity. */
const LocationWorkspaceDetailPage = async function ({
  params,
  searchParams,
}: LocationWorkspaceDetailPageProps): Promise<JSX.Element> {
  const { locationId } = await params;
  const resolvedSearchParams = await searchParams;
  const workspaceParams = {
    ...(typeof resolvedSearchParams.search === "undefined"
      ? {}
      : { search: resolvedSearchParams.search }),
    ...(typeof resolvedSearchParams.sort === "undefined"
      ? {}
      : { sort: resolvedSearchParams.sort }),
    ...(typeof resolvedSearchParams.page === "undefined"
      ? {}
      : { page: resolvedSearchParams.page }),
    ...(typeof resolvedSearchParams.pageSize === "undefined"
      ? {}
      : { pageSize: resolvedSearchParams.pageSize }),
  } satisfies LocationWorkspaceSearchParams;
  const workspaceUrl = routes.workspace(workspaceParams);
  const transactionPage = normalizePageValue(
    resolvedSearchParams.transactionPage,
  );
  const rowsPerPage = getRowsPerPage(resolvedSearchParams.pageSize);
  const client = await createApiClient();
  const allLocationsResponse = await client.GET("/locations");
  const locations = unwrapApiResponse(
    allLocationsResponse,
    "Failed to fetch Locations",
  );
  const location = locations.items.find(
    (item) => item.id.toLowerCase() === locationId.toLowerCase(),
  );
  if (typeof location === "undefined") {
    redirect(workspaceUrl);
  }
  const transactionsResponse = await client.GET("/transactions", {
    params: {
      query: {
        "Filter.LocationIds": [location.id],
        Limit: rowsPerPage,
        Offset: getPageOffset(transactionPage, rowsPerPage),
      },
    },
  });
  const transactions = unwrapApiResponse(
    transactionsResponse,
    "Failed to fetch Location transactions",
  );
  const defaultEndDate = dayjs();
  const defaultStartDate = defaultEndDate.subtract(90, "day");
  const cashFlow = unwrapApiResponse(
    await client.GET("/transactions/date-range", {
      params: {
        query: {
          "Filter.LocationIds": [location.id],
          "Range.Start": defaultStartDate.format("YYYY-MM-DD"),
          "Range.End": defaultEndDate.format("YYYY-MM-DD"),
          Limit: 500,
        },
      },
    }),
    "Failed to fetch Location cash flow",
  );
  const incomingTotal = cashFlow.locationIncomingAmount;
  const outgoingTotal = cashFlow.locationOutgoingAmount;
  const currentUrl = routes.workspaceDetail(location.id, {
    ...workspaceParams,
    ...(typeof resolvedSearchParams.transactionPage === "undefined"
      ? {}
      : { transactionPage: resolvedSearchParams.transactionPage }),
  });
  const transactionWorkspaceUrl = transactionRoutes.workspace({
    locationIds: [location.id],
    returnUrl: currentUrl,
  });

  return (
    <PageLayout>
      <ResponsivePageSize desktopBreakpoint="lg" />
      <Box sx={{ maxWidth: 1200, width: "100%" }}>
        <Stack spacing={2.5}>
          <Link
            href={workspaceUrl}
            style={{ alignSelf: "flex-start", textDecoration: "none" }}
          >
            <Button component="span" startIcon={<ArrowBack />}>
              Back to Workspace
            </Button>
          </Link>
          <Stack
            direction={{ xs: "column", sm: "row" }}
            justifyContent="space-between"
            spacing={2}
          >
            <Typography variant="h4">{location.name}</Typography>
            <LocationDetailActions
              location={location}
              locations={locations.items}
              workspaceUrl={workspaceUrl}
            />
          </Stack>
          <ResponsiveGrid columns={{ xs: 1, md: 2 }}>
            <SummaryCard
              title="Incoming Money"
              value={`$ ${incomingTotal.toFixed(2)}`}
            />
            <SummaryCard
              title="Outgoing Money"
              value={`$ ${outgoingTotal.toFixed(2)}`}
            />
          </ResponsiveGrid>
          <LocationCashFlowChart
            transactions={cashFlow.transactions.items}
            mode="Date"
            startDate={defaultStartDate.format("YYYY-MM-DD")}
            endDate={defaultEndDate.format("YYYY-MM-DD")}
          />
          <RecentLocationTransactionsFrame
            transactions={transactions.items}
            totalCount={transactions.totalCount}
            locationId={location.id}
            returnUrl={currentUrl}
            headerContent={
              <Link
                href={transactionWorkspaceUrl}
                style={{ textDecoration: "none" }}
              >
                <Button component="span" variant="outlined">
                  View All Transactions
                </Button>
              </Link>
            }
          />
        </Stack>
      </Box>
    </PageLayout>
  );
};

export default LocationWorkspaceDetailPage;
