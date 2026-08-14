"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
} from "@/accounting-periods/types";
import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import { useRouter, useSearchParams } from "next/navigation";
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
      getBodyContent: (source) => formatCurrency(source.netAmount.total),
      alignment: "right",
    },
    {
      name: "expectedAmount",
      headerContent: "Expected Income",
      getBodyContent: (source) => formatCurrency(source.expectedAmount.total),
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
      pageParamName="incomeSourcePage"
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
