"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
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
import { formatCurrency } from "@/framework/currencyHelpers";
import routes from "@/accounting-periods/routes";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the ExpectedIncomeSourcesFrame component.
 */
interface ExpectedIncomeSourcesFrameProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly routeSearchParams: AccountingPeriodWorkspaceSearchParams;
}

/**
 * Displays expected income sources and actions to manage individual sources.
 */
const ExpectedIncomeSourcesFrame = function ({
  accountingPeriod,
  routeSearchParams,
}: ExpectedIncomeSourcesFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const canManageSources = canWrite && accountingPeriod.isOpen;
  const router = useRouter();
  const searchParams = useSearchParams();
  const sources = accountingPeriod.expectedIncomeSources;
  const rowsPerPage = getRowsPerPage(searchParams.get("pageSize"));
  const paginationIndex = getPaginationIndex(
    searchParams.get("incomeSourcePage"),
    sources.length,
    rowsPerPage,
  );
  const paginatedSources = sources.slice(
    paginationIndex * rowsPerPage,
    (paginationIndex + 1) * rowsPerPage,
  );
  const columns: ColumnDefinition<ExpectedIncomeSource>[] = [
    {
      name: "name",
      headerContent: "Source",
      getBodyContent: (source) => source.name,
      mobilePrimary: true,
    },
    {
      name: "payments",
      headerContent: "Expected Payments",
      getBodyContent: (source) => source.expectedDates.length,
      alignment: "right",
    },
    {
      name: "netAmount",
      headerContent: "Per Payment",
      getBodyContent: (source) => formatCurrency(source.trackedAmount),
      alignment: "right",
    },
    {
      name: "expectedAmount",
      headerContent: "Expected Income",
      getBodyContent: (source) => formatCurrency(source.expectedAmount),
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
              routes.expectedIncomeDetail(
                accountingPeriod.id,
                source.id,
                routeSearchParams,
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
          <Link
            href={routes.expectedIncomeCreate(
              accountingPeriod.id,
              routeSearchParams,
            )}
            style={{ textDecoration: "none" }}
          >
            <Button component="span" variant="contained">
              Add Expected Income
            </Button>
          </Link>
        )
      }
      columns={columns}
      getId={(source) => source.id}
      data={paginatedSources}
      totalCount={sources.length}
      pageParamName="incomeSourcePage"
      onRowClick={(source) => {
        router.push(
          routes.expectedIncomeDetail(
            accountingPeriod.id,
            source.id,
            routeSearchParams,
          ),
        );
      }}
      initialEmptyState={{
        title: "No expected income sources",
        description:
          "Add income sources to plan this period's expected income.",
        action: null,
      }}
    />
  );
};

export default ExpectedIncomeSourcesFrame;
