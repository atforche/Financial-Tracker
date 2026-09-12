"use client";

import {
  type AccountingPeriodWithBalance,
  type ExpectedIncomeSource,
  ExpectedIncomeSourceSort,
} from "@/accounting-periods/types";
import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import { useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriodWorkspaceSearchParams } from "@/accounting-periods/workspace/AccountingPeriodWorkspace";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/accounting-periods/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourcesFrame component.
 */
interface ExpectedIncomeSourcesFrameProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly redirectUrl: string;
}

/**
 * Displays expected income sources and actions to manage individual sources.
 */
const ExpectedIncomeSourcesFrame = function ({
  accountingPeriod,
  redirectUrl,
}: ExpectedIncomeSourcesFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const router = useRouter();
  const canManageSources = canWrite && accountingPeriod.isOpen;
  const searchParams = useSearchParams();
  const sources = accountingPeriod.expectedIncomeSources;
  const sortParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("incomeSourceSort");
  const pageParamName =
    propertyName<AccountingPeriodWorkspaceSearchParams>("incomeSourcePage");
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    ExpectedIncomeSourceSort,
    searchParams.get(sortParamName) ?? "",
  );
  const setSort = function (sort: ExpectedIncomeSourceSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };
  const sortedSources =
    currentSort === null
      ? sources
      : sources.toSorted((left, right) => {
          const direction =
            currentSort === ExpectedIncomeSourceSort.SourceDescending ||
            currentSort ===
              ExpectedIncomeSourceSort.ExpectedTrackedIncomeDescending ||
            currentSort ===
              ExpectedIncomeSourceSort.ExpectedUntrackedIncomeDescending
              ? -1
              : 1;
          const comparison =
            currentSort === ExpectedIncomeSourceSort.Source ||
            currentSort === ExpectedIncomeSourceSort.SourceDescending
              ? left.name.localeCompare(right.name)
              : currentSort ===
                    ExpectedIncomeSourceSort.ExpectedTrackedIncome ||
                  currentSort ===
                    ExpectedIncomeSourceSort.ExpectedTrackedIncomeDescending
                ? left.expectedAmount.tracked - right.expectedAmount.tracked
                : left.expectedAmount.untracked -
                  right.expectedAmount.untracked;
          return comparison === 0
            ? left.id.localeCompare(right.id)
            : comparison * direction;
        });
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const paginationIndex = getPaginationIndex(
    searchParams.get(pageParamName),
    sources.length,
    rowsPerPage,
  );
  const paginatedSources = sortedSources.slice(
    paginationIndex * rowsPerPage,
    (paginationIndex + 1) * rowsPerPage,
  );
  const getSortProps = createColumnSortProps(currentSort, setSort);
  const columns: ColumnDefinition<ExpectedIncomeSource>[] = [
    {
      name: "name",
      headerContent: "Source",
      getBodyContent: (source) => source.name,
      mobilePrimary: true,
      ...getSortProps(
        ExpectedIncomeSourceSort.Source,
        ExpectedIncomeSourceSort.SourceDescending,
      ),
    },
    {
      name: "expectedTrackedIncome",
      headerContent: "Tracked Income",
      getBodyContent: (source) => formatCurrency(source.expectedAmount.tracked),
      ...getSortProps(
        ExpectedIncomeSourceSort.ExpectedTrackedIncome,
        ExpectedIncomeSourceSort.ExpectedTrackedIncomeDescending,
      ),
      alignment: "right",
    },
    {
      name: "expectedUntrackedIncome",
      headerContent: "Untracked Income",
      getBodyContent: (source) =>
        formatCurrency(source.expectedAmount.untracked),
      ...getSortProps(
        ExpectedIncomeSourceSort.ExpectedUntrackedIncome,
        ExpectedIncomeSourceSort.ExpectedUntrackedIncomeDescending,
      ),
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (source) => (
        <ListFrameActionButton
          ariaLabel={`View ${source.name}`}
          onClick={() => {
            router.push(
              routes.expectedIncomeSource(
                accountingPeriod.id,
                source.id,
                redirectUrl,
              ),
            );
          }}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];
  return (
    <ListFrame
      title="Expected Income Sources"
      headerContent={
        !canManageSources ? undefined : (
          <Button
            component={Link}
            href={routes.expectedIncomeSourceCreate(
              accountingPeriod.id,
              redirectUrl,
            )}
            variant="contained"
          >
            Add Expected Income
          </Button>
        )
      }
      columns={columns}
      getId={(source) => source.id}
      data={paginatedSources}
      totalCount={sources.length}
      pageParamName={pageParamName}
      onRowClick={(source) => {
        router.push(
          routes.expectedIncomeSource(
            accountingPeriod.id,
            source.id,
            redirectUrl,
          ),
        );
      }}
      initialEmptyState={{
        title: "No Expected Income Sources",
        description:
          "Add income sources to plan this period's expected income.",
        action: null,
      }}
    />
  );
};

export default ExpectedIncomeSourcesFrame;
