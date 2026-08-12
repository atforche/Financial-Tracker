"use client";

import { Add, Delete } from "@mui/icons-material";
import {
  Button,
  Card,
  CardContent,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import dayjs from "dayjs";

/**
 * Props for the ExpectedIncomeSourcesEditor component.
 */
interface ExpectedIncomeSourcesEditorProps {
  readonly sources: ExpectedIncomeSourceRequest[];
  readonly setSources: (sources: ExpectedIncomeSourceRequest[]) => void;
  readonly year: number | null;
  readonly month: number | null;
  readonly showSourceControls?: boolean;
}

/**
 * Creates an empty expected income source request object.
 */
const emptySource = (): ExpectedIncomeSourceRequest => ({
  name: "",
  incomeLines: [{ description: "Income", amount: 0 }],
  incomeDeductions: [],
  expectedDates: [],
});

/**
 * Edits expected-income configuration for an Accounting Period.
 */
const ExpectedIncomeSourcesEditor = function ({
  sources,
  setSources,
  year,
  month,
  showSourceControls = true,
}: ExpectedIncomeSourcesEditorProps): JSX.Element {
  const updateSource = (
    index: number,
    source: ExpectedIncomeSourceRequest,
  ): void => {
    setSources(
      sources.map((item, itemIndex) => (itemIndex === index ? source : item)),
    );
  };
  const minDate =
    year === null || month === null
      ? null
      : dayjs(new Date(year, month - 1, 1));
  const maxDate = minDate?.endOf("month") ?? null;

  return (
    <Stack spacing={1.5}>
      <Typography variant="subtitle1">Expected Income Sources</Typography>
      <Typography variant="body2" color="text.secondary">
        Set the net amount for each expected payment and the dates it will be
        received.
      </Typography>
      {sources.map((source, sourceIndex) => (
        <Card key={sourceIndex} variant="outlined">
          <CardContent>
            <Stack spacing={1.5}>
              <Stack direction="row" spacing={1} alignItems="center">
                <StringEntryField
                  label="Source Name"
                  value={source.name}
                  setValue={(name) => {
                    updateSource(sourceIndex, { ...source, name });
                  }}
                />
                {showSourceControls ? (
                  <IconButton
                    aria-label="Remove expected income source"
                    onClick={() => {
                      setSources(
                        sources.filter((_, index) => index !== sourceIndex),
                      );
                    }}
                  >
                    <Delete />
                  </IconButton>
                ) : null}
              </Stack>
              <Typography variant="body2">Income Lines</Typography>
              {source.incomeLines.map((line, lineIndex) => (
                <Stack key={lineIndex} direction="row" spacing={1}>
                  <StringEntryField
                    label="Description"
                    value={line.description}
                    setValue={(description) => {
                      const incomeLines = source.incomeLines.map(
                        (item, index) =>
                          index === lineIndex ? { ...item, description } : item,
                      );
                      updateSource(sourceIndex, { ...source, incomeLines });
                    }}
                  />
                  <CurrencyEntryField
                    label="Amount"
                    value={line.amount}
                    setValue={(amount) => {
                      const incomeLines = source.incomeLines.map(
                        (item, index) =>
                          index === lineIndex
                            ? { ...item, amount: amount ?? 0 }
                            : item,
                      );
                      updateSource(sourceIndex, { ...source, incomeLines });
                    }}
                  />
                  <IconButton
                    aria-label="Remove income line"
                    disabled={source.incomeLines.length === 1}
                    onClick={() => {
                      updateSource(sourceIndex, {
                        ...source,
                        incomeLines: source.incomeLines.filter(
                          (_, index) => index !== lineIndex,
                        ),
                      });
                    }}
                  >
                    <Delete />
                  </IconButton>
                </Stack>
              ))}
              <Button
                startIcon={<Add />}
                onClick={() => {
                  updateSource(sourceIndex, {
                    ...source,
                    incomeLines: [
                      ...source.incomeLines,
                      { description: "", amount: 0 },
                    ],
                  });
                }}
              >
                Add Income Line
              </Button>
              <Typography variant="body2">Deductions</Typography>
              {source.incomeDeductions.map((deduction, deductionIndex) => (
                <Stack key={deductionIndex} direction="row" spacing={1}>
                  <StringEntryField
                    label="Description"
                    value={deduction.description}
                    setValue={(description) => {
                      const incomeDeductions = source.incomeDeductions.map(
                        (item, index) =>
                          index === deductionIndex
                            ? { ...item, description }
                            : item,
                      );
                      updateSource(sourceIndex, {
                        ...source,
                        incomeDeductions,
                      });
                    }}
                  />
                  <CurrencyEntryField
                    label="Amount"
                    value={deduction.amount}
                    setValue={(amount) => {
                      const incomeDeductions = source.incomeDeductions.map(
                        (item, index) =>
                          index === deductionIndex
                            ? { ...item, amount: amount ?? 0 }
                            : item,
                      );
                      updateSource(sourceIndex, {
                        ...source,
                        incomeDeductions,
                      });
                    }}
                  />
                  <IconButton
                    aria-label="Remove deduction"
                    onClick={() => {
                      updateSource(sourceIndex, {
                        ...source,
                        incomeDeductions: source.incomeDeductions.filter(
                          (_, index) => index !== deductionIndex,
                        ),
                      });
                    }}
                  >
                    <Delete />
                  </IconButton>
                </Stack>
              ))}
              <Button
                startIcon={<Add />}
                onClick={() => {
                  updateSource(sourceIndex, {
                    ...source,
                    incomeDeductions: [
                      ...source.incomeDeductions,
                      { description: "", amount: 0 },
                    ],
                  });
                }}
              >
                Add Deduction
              </Button>
              <Typography variant="body2">Expected Payment Dates</Typography>
              {source.expectedDates.map((date, dateIndex) => (
                <Stack key={`${date}-${dateIndex}`} direction="row" spacing={1}>
                  <DateEntryField
                    label="Expected Date"
                    value={dayjs(date)}
                    minDate={minDate}
                    maxDate={maxDate}
                    setValue={(value) => {
                      const expectedDates = source.expectedDates.map(
                        (item, index) =>
                          index === dateIndex
                            ? (value?.format("YYYY-MM-DD") ?? item)
                            : item,
                      );
                      updateSource(sourceIndex, { ...source, expectedDates });
                    }}
                  />
                  <IconButton
                    aria-label="Remove expected date"
                    onClick={() => {
                      updateSource(sourceIndex, {
                        ...source,
                        expectedDates: source.expectedDates.filter(
                          (_, index) => index !== dateIndex,
                        ),
                      });
                    }}
                  >
                    <Delete />
                  </IconButton>
                </Stack>
              ))}
              <Button
                startIcon={<Add />}
                disabled={minDate === null}
                onClick={() => {
                  updateSource(sourceIndex, {
                    ...source,
                    expectedDates: [
                      ...source.expectedDates,
                      minDate?.format("YYYY-MM-DD") ?? "",
                    ],
                  });
                }}
              >
                Add Expected Date
              </Button>
            </Stack>
          </CardContent>
        </Card>
      ))}
      {showSourceControls ? (
        <Button
          variant="outlined"
          startIcon={<Add />}
          onClick={() => {
            setSources([...sources, emptySource()]);
          }}
        >
          Add Expected Income Source
        </Button>
      ) : null}
    </Stack>
  );
};

export default ExpectedIncomeSourcesEditor;
