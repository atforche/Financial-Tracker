"use client";

import { Divider, Stack } from "@mui/material";
import ExpectedIncomeAmountEditor from "@/accounting-periods/workspace/ExpectedIncomeAmountEditor";
import ExpectedIncomeDatesEditor from "@/accounting-periods/workspace/ExpectedIncomeDatesEditor";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import type { JSX } from "react";

/**
 * Props for the ExpectedIncomeSourcesEditor component.
 */
interface ExpectedIncomeSourcesEditorProps {
  readonly source: ExpectedIncomeSourceRequest;
  readonly setSource: (source: ExpectedIncomeSourceRequest) => void;
  readonly year: number;
  readonly month: number;
}

/**
 * Displays the editable dates, income lines, deductions, and untracked transfers for one expected-income source.
 */
const ExpectedIncomeSourcesEditor = function ({
  source,
  setSource,
  year,
  month,
}: ExpectedIncomeSourcesEditorProps): JSX.Element {
  return (
    <Stack spacing={1.5}>
      <ExpectedIncomeDatesEditor
        dates={source.expectedDates}
        setDates={(expectedDates): void => {
          setSource({ ...source, expectedDates });
        }}
        year={year}
        month={month}
      />
      <Divider />
      <ExpectedIncomeAmountEditor
        title="Income Lines"
        description="Add the gross income amounts that make up each expected payment."
        addLabel="Add Income Line"
        requireItem
        items={source.incomeLines}
        setItems={(incomeLines): void => {
          setSource({ ...source, incomeLines });
        }}
      />
      <Divider />
      <ExpectedIncomeAmountEditor
        title="Deductions"
        description="Add optional deductions withheld before the income is deposited."
        addLabel="Add Deduction"
        items={source.incomeDeductions}
        setItems={(incomeDeductions): void => {
          setSource({ ...source, incomeDeductions });
        }}
      />
      <Divider />
      <ExpectedIncomeAmountEditor
        title="Untracked Transfers"
        description="Add amounts expected to remain outside tracked accounts."
        addLabel="Add Untracked Transfer"
        items={source.untrackedTransfers}
        setItems={(untrackedTransfers): void => {
          setSource({ ...source, untrackedTransfers });
        }}
      />
    </Stack>
  );
};

export default ExpectedIncomeSourcesEditor;
