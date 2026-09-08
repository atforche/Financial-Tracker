"use client";

import { Divider, Stack } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ExpectedIncomeAmountEditor from "@/accounting-periods/workspace/ExpectedIncomeAmountEditor";
import ExpectedIncomeDatesEditor from "@/accounting-periods/workspace/ExpectedIncomeDatesEditor";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import Frame from "@/framework/view/Frame";
import StringEntryField from "@/framework/forms/StringEntryField";

interface ExpectedIncomeSourcesEditorProps {
  readonly source: ExpectedIncomeSourceRequest;
  readonly setSource?: ((source: ExpectedIncomeSourceRequest) => void) | null;
  readonly year: number | null;
  readonly month: number | null;
  readonly readOnly?: boolean;
  readonly headerContent?: ReactNode;
}

/** Displays the shared editable or read-only fields for one expected-income source. */
const ExpectedIncomeSourcesEditor = function ({
  source,
  setSource = null,
  year,
  month,
  readOnly = false,
  headerContent,
}: ExpectedIncomeSourcesEditorProps): JSX.Element {
  const editable = !readOnly && setSource !== null;
  const updateSource = (nextSource: ExpectedIncomeSourceRequest): void => {
    setSource?.(nextSource);
  };

  return (
    <Frame
      title={source.name || "Expected Income Source"}
      color="info"
      headerContent={headerContent}
    >
      <Stack spacing={1.5}>
        <StringEntryField
          label="Source Name"
          value={source.name}
          setValue={
            editable
              ? (name): void => {
                  updateSource({ ...source, name });
                }
              : null
          }
        />
        <Divider />
        <ExpectedIncomeAmountEditor
          title="Income Lines"
          description="Add the gross income amounts that make up each expected payment."
          addLabel="Add Income Line"
          requireItem
          items={source.incomeLines}
          setItems={
            editable
              ? (incomeLines): void => {
                  updateSource({ ...source, incomeLines });
                }
              : null
          }
        />
        <Divider />
        <ExpectedIncomeAmountEditor
          title="Deductions"
          description="Add optional deductions withheld before the income is deposited."
          addLabel="Add Deduction"
          items={source.incomeDeductions}
          setItems={
            editable
              ? (incomeDeductions): void => {
                  updateSource({ ...source, incomeDeductions });
                }
              : null
          }
        />
        <Divider />
        <ExpectedIncomeAmountEditor
          title="Untracked Transfers"
          description="Add amounts expected to remain outside tracked accounts."
          addLabel="Add Untracked Transfer"
          items={source.untrackedTransfers}
          setItems={
            editable
              ? (untrackedTransfers): void => {
                  updateSource({ ...source, untrackedTransfers });
                }
              : null
          }
        />
        <Divider />
        <ExpectedIncomeDatesEditor
          dates={source.expectedDates}
          setDates={
            editable
              ? (expectedDates): void => {
                  updateSource({ ...source, expectedDates });
                }
              : null
          }
          year={year}
          month={month}
        />
      </Stack>
    </Frame>
  );
};

export default ExpectedIncomeSourcesEditor;
