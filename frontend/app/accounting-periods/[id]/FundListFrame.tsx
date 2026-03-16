"use client";

import { AccountingPeriodFundSortOrder, type Fund } from "@/data/fundTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@/framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the FundListFrame component.
 */
interface FundListFrameProps {
  readonly data: Fund[] | null;
  readonly totalCount: number | null;
}

/**
 * Component that displays the list of funds associated with an accounting period.
 */
const FundListFrame = function ({
  data,
  totalCount,
}: FundListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const setSort = function (sort: AccountingPeriodFundSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete("sort");
    } else {
      params.set("sort", sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountingPeriodFundSortOrder,
    searchParams.get("sort") ?? "",
  );

  const columns: ColumnDefinition<Fund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund: Fund) => fund.name,
      sortType:
        currentSort === AccountingPeriodFundSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountingPeriodFundSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "openingBalance",
      headerContent: "Opening Balance",
      getBodyContent: () => "TBD",
      sortType:
        currentSort === AccountingPeriodFundSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "closingBalance",
      headerContent: "Closing Balance",
      getBodyContent: () => "TBD",
      sortType:
        currentSort === AccountingPeriodFundSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort ===
              AccountingPeriodFundSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountingPeriodFundSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountingPeriodFundSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(`${pathname}/fund/create`);
          }}
        />
      ),
      getBodyContent: (fund: Fund) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(`${pathname}/fund/${fund.id}`);
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
