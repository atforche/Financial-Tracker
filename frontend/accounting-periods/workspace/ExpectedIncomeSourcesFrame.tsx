"use client";

import type {
  AccountingPeriodWithBalance,
  ExpectedIncomeSource,
} from "@/accounting-periods/types";
import ExpectedIncomeSourceForm, {
  type ExpectedIncomeSourceMode,
} from "@/accounting-periods/workspace/ExpectedIncomeSourceForm";
import { type JSX, useState } from "react";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import DeleteOutline from "@mui/icons-material/DeleteOutline";
import EditOutlined from "@mui/icons-material/EditOutlined";
import ListFrame from "@/framework/listframe/ListFrame";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import { formatCurrency } from "@/framework/currencyHelpers";
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
  const [dialog, setDialog] = useState<{
    mode: ExpectedIncomeSourceMode;
    source?: ExpectedIncomeSource;
  } | null>(null);
  const sources = accountingPeriod.expectedIncomeSources;
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
      getBodyContent: (source) =>
        !canManageSources ? null : (
          <>
            <ListFrameActionButton
              ariaLabel={`Change ${source.name}`}
              onClick={() => {
                setDialog({ mode: "change", source });
              }}
            >
              <EditOutlined fontSize="small" color="action" />
            </ListFrameActionButton>
            <ListFrameActionButton
              ariaLabel={`Delete ${source.name}`}
              onClick={() => {
                setDialog({ mode: "delete", source });
              }}
            >
              <DeleteOutline fontSize="small" color="action" />
            </ListFrameActionButton>
          </>
        ),
      alignment: "right",
      minWidth: 96,
      maxWidth: 96,
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
        data={sources}
        totalCount={sources.length}
        pageParamName="incomeSourcePage"
        initialEmptyState={{
          title: "No expected income sources",
          description:
            "Add income sources to plan this period's expected income.",
          action: null,
        }}
      />
      {dialog === null ? null : (
        <ExpectedIncomeSourceForm
          accountingPeriod={accountingPeriod}
          mode={dialog.mode}
          {...(typeof dialog.source === "undefined"
            ? {}
            : { source: dialog.source })}
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
