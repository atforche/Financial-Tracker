"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
} from "@/accounting-periods/types";
import ExpectedIncomeSourceForm, {
  type ExpectedIncomeSourceMode,
} from "@/accounting-periods/workspace/ExpectedIncomeSourceForm";
import { type JSX, useState } from "react";
import { getPaginationIndex, getRowsPerPage } from "@/framework/listframe/page";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ExpectedIncomeSourceDetailsDialog from "@/accounting-periods/workspace/ExpectedIncomeSourceDetailsDialog";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import { formatCurrency } from "@/framework/currencyHelpers";
import { useSearchParams } from "next/navigation";
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
  const canManageSources = canWrite && accountingPeriod.isOpen;
  const searchParams = useSearchParams();
  const [dialog, setDialog] = useState<
    | { mode: "add" }
    | { mode: "view" | ExpectedIncomeSourceMode; source: ExpectedIncomeSource }
    | null
  >(null);
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
      getBodyContent: (source) => formatCurrency(source.netAmount),
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
          onClick={(event) => {
            event.stopPropagation();
            setDialog({ mode: "view", source });
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
    <>
      <ListFrame
        title="Expected Income Sources"
        headerContent={
          !canManageSources ? undefined : (
            <Button
              variant="contained"
              onClick={() => {
                setDialog({ mode: "add" });
              }}
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
          setDialog({ mode: "view", source });
        }}
        initialEmptyState={{
          title: "No expected income sources",
          description:
            "Add income sources to plan this period's expected income.",
          action: null,
        }}
      />
      {dialog?.mode === "view" ? (
        <ExpectedIncomeSourceDetailsDialog
          source={dialog.source}
          canManage={canManageSources}
          onClose={() => {
            setDialog(null);
          }}
          onChange={() => {
            setDialog({ mode: "change", source: dialog.source });
          }}
          onDelete={() => {
            setDialog({ mode: "delete", source: dialog.source });
          }}
        />
      ) : dialog === null ? null : (
        <ExpectedIncomeSourceForm
          accountingPeriod={accountingPeriod}
          mode={dialog.mode}
          {...("source" in dialog ? { source: dialog.source } : {})}
          open
          onClose={() => {
            setDialog(null);
          }}
          redirectUrl={redirectUrl}
        />
      )}
    </>
  );
};

export default ExpectedIncomeSourcesFrame;
