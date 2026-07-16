"use client";

import { Box, Button, IconButton, Stack } from "@mui/material";
import {
  type FundWithBalanceRange,
  FundWithBalanceRangeSort,
} from "@/funds/types";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import routes from "@/funds/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

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
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const fundNameParamName = "fundName";
  const modeParamName = "mode";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";
  const updateParams = useSearchParamUpdater([pageParamName]);

  const setSort = function (sort: FundWithBalanceRangeSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const setFundNameFilter = function (fundName: string): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      params.append(fundNameParamName, fundName);
    });
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

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: ColumnDefinition<FundWithBalanceRange>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund) => fund.name,
      ...getSortProps(
        FundWithBalanceRangeSort.Name,
        FundWithBalanceRangeSort.NameDescending,
      ),
    },
    {
      name: "startingBalance",
      headerContent: "Starting Balance",
      getBodyContent: (fund) => formatCurrency(fund.startingBalance),
      ...getSortProps(
        FundWithBalanceRangeSort.StartingBalance,
        FundWithBalanceRangeSort.StartingBalanceDescending,
      ),
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "endingBalance",
      headerContent: "Ending Balance",
      getBodyContent: (fund) => formatCurrency(fund.endingBalance),
      ...getSortProps(
        FundWithBalanceRangeSort.EndingBalance,
        FundWithBalanceRangeSort.EndingBalanceDescending,
      ),
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
      ...getSortProps(
        FundWithBalanceRangeSort.NetChange,
        FundWithBalanceRangeSort.NetChangeDescending,
      ),
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
    <ListFrame<FundWithBalanceRange>
      title="Funds"
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
              updateParams((params) => {
                [...params.keys()].forEach((key) => {
                  params.delete(key);
                });
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }}
    />
  );
};

export default FundTrendsListFrame;
