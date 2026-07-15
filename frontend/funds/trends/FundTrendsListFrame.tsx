"use client";

import {
  Box,
  Button,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import {
  type FundWithBalanceRange,
  FundWithBalanceRangeSort,
  type FundWithBalanceRangeSortValue,
} from "@/funds/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/funds/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the FundTrendsListFrame component.
 */
interface FundTrendsListFrameProps {
  readonly data: FundWithBalanceRange[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged fund table for the Funds trends.
 */
const FundTrendsListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: FundTrendsListFrameProps): JSX.Element {
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

  const setSort = function (sort: FundWithBalanceRangeSortValue | null): void {
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

  const openFundWorkspace = function (fund: FundWithBalanceRange): void {
    router.push(routes.workspaceDetail(fund.id, {}));
  };

  const currentSort = tryParseEnum(
    FundWithBalanceRangeSort,
    searchParams.get(sortParamName) ?? "",
  );
  const hasActiveFilters =
    searchParams.getAll(fundNameParamName).length > 0 ||
    searchParams.get(modeParamName) === "date" ||
    searchParams.has(startAccountingPeriodIdParamName) ||
    searchParams.has(endAccountingPeriodIdParamName) ||
    searchParams.has(startDateParamName) ||
    searchParams.has(endDateParamName);

  const columns: ColumnDefinition<FundWithBalanceRange>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund) => fund.name,
      sortType:
        currentSort === FundWithBalanceRangeSort.Name
          ? ColumnSortType.Ascending
          : currentSort === FundWithBalanceRangeSort.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundWithBalanceRangeSort.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundWithBalanceRangeSort.NameDescending);
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
        currentSort === FundWithBalanceRangeSort.StartingBalance
          ? ColumnSortType.Ascending
          : currentSort === FundWithBalanceRangeSort.StartingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundWithBalanceRangeSort.StartingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundWithBalanceRangeSort.StartingBalanceDescending);
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
        currentSort === FundWithBalanceRangeSort.EndingBalance
          ? ColumnSortType.Ascending
          : currentSort === FundWithBalanceRangeSort.EndingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundWithBalanceRangeSort.EndingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundWithBalanceRangeSort.EndingBalanceDescending);
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
        currentSort === FundWithBalanceRangeSort.NetChange
          ? ColumnSortType.Ascending
          : currentSort === FundWithBalanceRangeSort.NetChangeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundWithBalanceRangeSort.NetChange);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundWithBalanceRangeSort.NetChangeDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (fund) => (
        <Stack direction="row" spacing={0.5} justifyContent="flex-end">
          <IconButton
            size="small"
            color="inherit"
            onClick={(event) => {
              event.stopPropagation();
              setFundNameFilter(fund.name);
            }}
            aria-label={`Filter ${fund.name}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </IconButton>
          <IconButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openFundWorkspace(fund);
            }}
            aria-label={`Open ${fund.name}`}
          >
            <ArrowForwardOutlined fontSize="small" color="action" />
          </IconButton>
        </Stack>
      ),
      alignment: "right",
      minWidth: 84,
      maxWidth: 84,
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
        <ListFrame<FundWithBalanceRange>
          columns={columns}
          getId={(fund) => fund.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName="search"
          pageParamName={pageParamName}
          onRowClick={(fund: FundWithBalanceRange): void => {
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
                      ? routes.workspaceOnboard({})
                      : routes.workspaceCreate({}),
                  );
                }}
              >
                {isInOnboardingMode ? "Onboard fund" : "Create fund"}
              </Button>
            ),
          }}
          filteredEmptyState={{
            title: "No accounts match this trends filter",
            description:
              "Try a different account type, account name, or date range to widen the trends scope.",
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

export default FundTrendsListFrame;
