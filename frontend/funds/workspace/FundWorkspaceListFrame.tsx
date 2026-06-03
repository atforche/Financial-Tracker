"use client";

import { Button, Checkbox, Paper } from "@mui/material";
import { type Fund, FundSortOrder } from "@/funds/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the FundWorkspaceListFrame component.
 */
interface FundWorkspaceListFrameProps {
  readonly data: Fund[] | null;
  readonly totalCount: number | null;
  readonly selectedFundId: string | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays the paged fund list for the workspace.
 */
const FundWorkspaceListFrame = function ({
  data,
  totalCount,
  selectedFundId,
  isInOnboardingMode,
}: FundWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const selectedFundIdParamName = "selectedFundId";
  const actionParamName = "action";
  const searchParamName = "search";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const setSort = function (sort: FundSortOrder | null): void {
    replaceSearchParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const toggleSelection = function (fundId: string): void {
    replaceSearchParams((params) => {
      const currentlySelectedFundId = params.get(selectedFundIdParamName);
      if (currentlySelectedFundId === fundId) {
        params.delete(selectedFundIdParamName);
        return;
      }
      params.set(selectedFundIdParamName, fundId);
    });
  };

  const currentSort = tryParseEnum(
    FundSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<Fund>[] = [
    {
      name: "selected",
      headerContent: "",
      getBodyContent: (fund) => (
        <Checkbox
          checked={selectedFundId === fund.id}
          onClick={(event) => {
            event.stopPropagation();
            toggleSelection(fund.id);
          }}
          slotProps={{
            input: {
              "aria-label": `Select ${fund.name}`,
            },
          }}
        />
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund) => fund.name,
      sortType:
        currentSort === FundSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === FundSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "balance",
      headerContent: "Current Balance",
      getBodyContent: (fund) =>
        formatCurrency(fund.currentBalance.postedBalance),
      sortType:
        currentSort === FundSortOrder.Balance
          ? ColumnSortType.Ascending
          : currentSort === FundSortOrder.BalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundSortOrder.Balance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundSortOrder.BalanceDescending);
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
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
      }}
    >
      <ListFrame<Fund>
        columns={columns}
        getId={(fund) => fund.id}
        data={data ?? null}
        totalCount={totalCount ?? null}
        searchParamName={searchParamName}
        pageParamName={pageParamName}
        onRowClick={(fund) => {
          toggleSelection(fund.id);
        }}
        isRowSelected={(fund) => fund.id === selectedFundId}
        initialEmptyState={{
          title: "No funds yet",
          description: isInOnboardingMode
            ? "Use the onboarding action to add the first fund."
            : "Use the create action to add the first fund.",
          action: null,
        }}
        filteredEmptyState={{
          title: "No funds match this search",
          description:
            "Try a different name, type, or balance search to widen the list.",
          action: (
            <Button
              variant="contained"
              onClick={() => {
                replaceSearchParams((params) => {
                  params.delete(searchParamName);
                  params.delete(pageParamName);
                  params.delete(selectedFundIdParamName);
                  if (
                    params.get(actionParamName) === "update" ||
                    params.get(actionParamName) === "delete"
                  ) {
                    params.delete(actionParamName);
                  }
                });
              }}
            >
              Clear search
            </Button>
          ),
        }}
      />
    </Paper>
  );
};

export default FundWorkspaceListFrame;
