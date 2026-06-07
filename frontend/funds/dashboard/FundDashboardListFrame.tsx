"use client";

import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import { type FundDashboardFund, FundDashboardSortOrder } from "@/funds/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/accounts/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the FundDashboardListFrame component.
 */
interface FundDashboardListFrameProps {
  readonly data: FundDashboardFund[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged fund table for the Funds dashboard.
 */
const FundDashboardListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: FundDashboardListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const fundNameParamName = "fundName";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const setSort = function (sort: FundDashboardSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const setFundNameFilter = function (fundName: string): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(fundNameParamName);
    params.append(fundNameParamName, fundName);
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
  };

  const currentSort = tryParseEnum(
    FundDashboardSortOrder,
    searchParams.get(sortParamName) ?? "",
  );
  const hasActiveFilters =
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.get(modeParamName) === "date" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns: ColumnDefinition<FundDashboardFund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund) => fund.name,
      sortType:
        currentSort === FundDashboardSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "startingBalance",
      headerContent: "Starting Balance",
      getBodyContent: (fund) => formatCurrency(fund.startingBalance),
      sortType:
        currentSort === FundDashboardSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "endingBalance",
      headerContent: "Ending Balance",
      getBodyContent: (fund) => formatCurrency(fund.endingBalance),
      sortType:
        currentSort === FundDashboardSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "change",
      headerContent: "Net Change",
      getBodyContent: (fund): JSX.Element => {
        const changeInBalance = fund.endingBalance - fund.startingBalance;
        const isPositive = changeInBalance >= 0;
        return (
          <Box
            component="span"
            sx={{
              color: isPositive ? "success.main" : "error.main",
              display: "inline",
            }}
          >
            {formatCurrency(changeInBalance)}
          </Box>
        );
      },
      sortType:
        currentSort === FundDashboardSortOrder.NetChange
          ? ColumnSortType.Ascending
          : currentSort === FundDashboardSortOrder.NetChangeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundDashboardSortOrder.NetChange);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundDashboardSortOrder.NetChangeDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 160,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Typography variant="h5">Funds</Typography>
        <ListFrame<FundDashboardFund>
          columns={columns}
          getId={(fund) => fund.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="search"
          pageParamName={pageParamName}
          onRowClick={(fund: FundDashboardFund): void => {
            setFundNameFilter(fund.name);
          }}
          hasActiveFilters={hasActiveFilters}
          initialEmptyState={{
            title: "No funds have been added",
            description: isInOnboardingMode
              ? "Onboard a new fund to start tracking balances."
              : "Create a new fund to start tracking balances.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.push(
                    isInOnboardingMode
                      ? routes.workspace({ action: "onboard" })
                      : routes.workspace({ action: "create" }),
                  );
                }}
              >
                {isInOnboardingMode ? "Onboard account" : "Create account"}
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No accounts match this dashboard filter",
            description:
              "Try a different account type, account name, or date range to widen the dashboard scope.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  router.replace(pathname);
                }}
              >
                Reset filters
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default FundDashboardListFrame;
