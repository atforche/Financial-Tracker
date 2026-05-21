"use client";

import { type Fund, FundSortOrder } from "@/funds/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import { Button } from "@mui/material";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { FundsViewSearchParams } from "@/funds/FundsView";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import nameof from "@/framework/data/nameof";
import routes from "@/funds/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the FundListFrame component.
 */
interface FundListFrameProps {
  readonly data: Fund[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Component that displays the list of funds.
 */
const FundListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: FundListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const searchParamName = nameof<FundsViewSearchParams>("search");
  const sortParamName = nameof<FundsViewSearchParams>("sort");
  const pageParamName = nameof<FundsViewSearchParams>("page");

  const setSort = function (sort: FundSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    FundSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<Fund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund: Fund) => fund.name,
      sortType:
        currentSort === FundSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === FundSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
      name: "description",
      headerContent: "Description",
      getBodyContent: (fund: Fund) => fund.description,
      sortType:
        currentSort === FundSortOrder.Description
          ? ColumnSortType.Ascending
          : currentSort === FundSortOrder.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundSortOrder.Description);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundSortOrder.DescriptionDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "balance",
      headerContent: "Balance",
      getBodyContent: (fund: Fund) =>
        formatCurrency(fund.currentBalance.postedBalance),
      sortType:
        currentSort === FundSortOrder.Balance
          ? ColumnSortType.Ascending
          : currentSort === FundSortOrder.BalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(FundSortOrder.Balance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(FundSortOrder.BalanceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(
              isInOnboardingMode ? routes.onboard : routes.create({}),
            );
          }}
        />
      ),
      getBodyContent: (fund: Fund) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(routes.detail({ id: fund.id }, {}));
          }}
        />
      ),
      alignment: "right",
    },
  ];

  return (
    <ListFrame<Fund>
      columns={columns}
      getId={(fund) => fund.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      searchParamName={searchParamName}
      pageParamName={pageParamName}
      initialEmptyState={{
        title: "No funds yet",
        description: isInOnboardingMode
          ? "Create your first fund during onboarding so you can start assigning money with intent."
          : "Create a fund to organize balances and track where money is assigned.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              router.push(
                isInOnboardingMode ? routes.onboard : routes.create({}),
              );
            }}
          >
            {isInOnboardingMode ? "Start onboarding" : "Create fund"}
          </Button>
        ),
      }}
      filteredEmptyState={{
        title: "No funds match this search",
        description:
          "Try a different fund name or clear the current search to see all funds.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              const params = new URLSearchParams(searchParams.toString());
              params.delete(searchParamName);
              params.delete(pageParamName);
              router.replace(`${pathname}?${params.toString()}`);
            }}
          >
            Clear search
          </Button>
        ),
      }}
    />
  );
};

export default FundListFrame;
