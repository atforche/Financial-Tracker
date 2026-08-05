"use client";

import { Box, Button, Stack } from "@mui/material";
import {
  type FundWithBalanceRange,
  FundWithBalanceRangeSort,
} from "@/funds/types";
import {
  clearFundTrendsFilters,
  fundTrendsParamNames,
  hasActiveFundTrendsFilters,
} from "@/funds/trends/helpers";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import FilterListOutlined from "@mui/icons-material/FilterListOutlined";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import parseEnumValue from "@/framework/data/parseEnumValue";
import routes from "@/funds/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

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
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();
  const router = useRouter();

  const sortParamName = fundTrendsParamNames.sort;
  const pageParamName = fundTrendsParamNames.page;
  const fundNameParamName = fundTrendsParamNames.fundName;
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

  const currentSort = parseEnumValue(
    FundWithBalanceRangeSort,
    searchParams.get(sortParamName) ?? "",
  );
  const hasActiveFilters = hasActiveFundTrendsFilters(searchParams);

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
        const changeInBalance = getCurrencyDifference(
          fund.endingBalance,
          fund.startingBalance,
        );
        const isPositive = compareCurrencyAmounts(changeInBalance, 0) >= 0;
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
          <ListFrameActionButton
            size="small"
            color="inherit"
            onClick={(event) => {
              event.stopPropagation();
              setFundNameFilter(fund.name);
            }}
            ariaLabel={`Filter ${fund.name}`}
          >
            <FilterListOutlined fontSize="small" color="action" />
          </ListFrameActionButton>
          <ListFrameActionButton
            size="small"
            color="primary"
            onClick={(event) => {
              event.stopPropagation();
              openFundWorkspace(fund);
            }}
            ariaLabel={`Open ${fund.name}`}
          >
            <ArrowForwardOutlined fontSize="small" color="action" />
          </ListFrameActionButton>
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
        action: !canWrite ? null : (
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
        title: "No funds match this trends filter",
        description:
          "Try a different fund name or date range to widen the trends scope.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                clearFundTrendsFilters(params);
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
