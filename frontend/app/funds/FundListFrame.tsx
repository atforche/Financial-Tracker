"use client";

import { type Fund, FundSortOrder } from "@/data/fundTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/framework/routes";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the FundListFrame component.
 */
interface FundListFrameProps {
  readonly data: Fund[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of funds.
 */
const FundListFrame = function ({
  data,
  totalCount,
}: FundListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: FundSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("sort");
    } else {
      params.set("sort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    FundSortOrder,
    searchParams.get("sort") ?? "",
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
            router.push(routes.funds.create);
          }}
        />
      ),
      getBodyContent: (fund: Fund) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(routes.funds.detail(fund.id));
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
    />
  );
};

export default FundListFrame;
