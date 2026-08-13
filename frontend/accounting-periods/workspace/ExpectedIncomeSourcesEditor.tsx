"use client";

import { AddCircleOutline, DeleteOutline } from "@mui/icons-material";
import {
  Box,
  Button,
  Divider,
  IconButton,
  Stack,
  Typography,
} from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import { alpha } from "@mui/material/styles";
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
  untrackedTransfers: [],
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
      {showSourceControls ? (
        <Typography variant="subtitle1">Expected Income Sources</Typography>
      ) : null}
      {sources.map((source, sourceIndex) => (
        <Frame
          key={sourceIndex}
          title={source.name || "Expected Income Source"}
          color="info"
        >
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
                  <DeleteOutline />
                </IconButton>
              ) : null}
            </Stack>
            <Divider />
            <Stack spacing={0.25}>
              <Typography variant="subtitle2">Income Lines</Typography>
              <Typography variant="body2" color="text.secondary">
                Add the gross income amounts that make up each expected payment.
              </Typography>
            </Stack>
            <Box
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: {
                  xs: "1fr",
                  md: "repeat(2, minmax(0, 1fr))",
                },
              }}
            >
              {source.incomeLines.map((line, lineIndex) => (
                <Box
                  key={lineIndex}
                  sx={{
                    display: "grid",
                    gap: 1.5,
                    gridTemplateColumns: {
                      xs: "1fr",
                      md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
                    },
                    alignItems: "start",
                    border: "1px solid",
                    borderColor: "divider",
                    borderRadius: 2,
                    p: 1.5,
                    backgroundColor: (theme) =>
                      alpha(theme.palette.info.main, 0.04),
                  }}
                >
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
                  <Box
                    sx={{
                      display: "flex",
                      justifyContent: { xs: "flex-end", md: "center" },
                      pt: { xs: 0, md: 1.25 },
                    }}
                  >
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
                      color="error"
                    >
                      <DeleteOutline />
                    </IconButton>
                  </Box>
                </Box>
              ))}
            </Box>
            <Button
              variant="outlined"
              startIcon={<AddCircleOutline />}
              sx={{ alignSelf: "flex-start" }}
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
            <Divider />
            <Stack spacing={0.25}>
              <Typography variant="subtitle2">Deductions</Typography>
              <Typography variant="body2" color="text.secondary">
                Add optional deductions withheld before the income is deposited.
              </Typography>
            </Stack>
            <Box
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: {
                  xs: "1fr",
                  md: "repeat(2, minmax(0, 1fr))",
                },
              }}
            >
              {source.incomeDeductions.map((deduction, deductionIndex) => (
                <Box
                  key={deductionIndex}
                  sx={{
                    display: "grid",
                    gap: 1.5,
                    gridTemplateColumns: {
                      xs: "1fr",
                      md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
                    },
                    alignItems: "start",
                    border: "1px solid",
                    borderColor: "divider",
                    borderRadius: 2,
                    p: 1.5,
                    backgroundColor: (theme) =>
                      alpha(theme.palette.info.main, 0.04),
                  }}
                >
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
                  <Box
                    sx={{
                      display: "flex",
                      justifyContent: { xs: "flex-end", md: "center" },
                      pt: { xs: 0, md: 1.25 },
                    }}
                  >
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
                      color="error"
                    >
                      <DeleteOutline />
                    </IconButton>
                  </Box>
                </Box>
              ))}
            </Box>
            <Button
              variant="outlined"
              startIcon={<AddCircleOutline />}
              sx={{ alignSelf: "flex-start" }}
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
            <Divider />
            <Stack spacing={0.25}>
              <Typography variant="subtitle2">Untracked Transfers</Typography>
              <Typography variant="body2" color="text.secondary">
                Add amounts expected to remain outside tracked accounts.
              </Typography>
            </Stack>
            <Box
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: {
                  xs: "1fr",
                  md: "repeat(2, minmax(0, 1fr))",
                },
              }}
            >
              {source.untrackedTransfers.map((transfer, transferIndex) => (
                <Box
                  key={transferIndex}
                  sx={{
                    display: "grid",
                    gap: 1.5,
                    gridTemplateColumns: {
                      xs: "1fr",
                      md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
                    },
                    alignItems: "start",
                    border: "1px solid",
                    borderColor: "divider",
                    borderRadius: 2,
                    p: 1.5,
                    backgroundColor: (theme) =>
                      alpha(theme.palette.info.main, 0.04),
                  }}
                >
                  <StringEntryField
                    label="Description"
                    value={transfer.description}
                    setValue={(description) => {
                      const untrackedTransfers = source.untrackedTransfers.map(
                        (item, index) =>
                          index === transferIndex
                            ? { ...item, description }
                            : item,
                      );
                      updateSource(sourceIndex, {
                        ...source,
                        untrackedTransfers,
                      });
                    }}
                  />
                  <CurrencyEntryField
                    label="Amount"
                    value={transfer.amount}
                    setValue={(amount) => {
                      const untrackedTransfers = source.untrackedTransfers.map(
                        (item, index) =>
                          index === transferIndex
                            ? { ...item, amount: amount ?? 0 }
                            : item,
                      );
                      updateSource(sourceIndex, {
                        ...source,
                        untrackedTransfers,
                      });
                    }}
                  />
                  <Box
                    sx={{
                      display: "flex",
                      justifyContent: { xs: "flex-end", md: "center" },
                      pt: { xs: 0, md: 1.25 },
                    }}
                  >
                    <IconButton
                      aria-label="Remove untracked transfer"
                      onClick={() => {
                        updateSource(sourceIndex, {
                          ...source,
                          untrackedTransfers: source.untrackedTransfers.filter(
                            (_, index) => index !== transferIndex,
                          ),
                        });
                      }}
                      color="error"
                    >
                      <DeleteOutline />
                    </IconButton>
                  </Box>
                </Box>
              ))}
            </Box>
            <Button
              variant="outlined"
              startIcon={<AddCircleOutline />}
              sx={{ alignSelf: "flex-start" }}
              onClick={() => {
                updateSource(sourceIndex, {
                  ...source,
                  untrackedTransfers: [
                    ...source.untrackedTransfers,
                    { description: "", amount: 0 },
                  ],
                });
              }}
            >
              Add Untracked Transfer
            </Button>
            <Divider />
            <Stack spacing={0.25}>
              <Typography variant="subtitle2">
                Expected Payment Dates
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Add the dates on which this source is expected to pay.
              </Typography>
            </Stack>
            <Box
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: "repeat(auto-fit, minmax(260px, 1fr))",
              }}
            >
              {source.expectedDates.map((date, dateIndex) => (
                <Box
                  key={`${date}-${dateIndex}`}
                  sx={{
                    display: "grid",
                    gap: 1.5,
                    gridTemplateColumns: {
                      xs: "1fr",
                      md: "minmax(0, 1fr) auto",
                    },
                    alignItems: "start",
                    border: "1px solid",
                    borderColor: "divider",
                    borderRadius: 2,
                    p: 1.5,
                    backgroundColor: (theme) =>
                      alpha(theme.palette.info.main, 0.04),
                  }}
                >
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
                  <Box
                    sx={{
                      display: "flex",
                      justifyContent: { xs: "flex-end", md: "center" },
                      pt: { xs: 0, md: 1.25 },
                    }}
                  >
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
                      color="error"
                    >
                      <DeleteOutline />
                    </IconButton>
                  </Box>
                </Box>
              ))}
            </Box>
            <Button
              variant="outlined"
              startIcon={<AddCircleOutline />}
              sx={{ alignSelf: "flex-start" }}
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
        </Frame>
      ))}
      {showSourceControls ? (
        <Button
          variant="outlined"
          startIcon={<AddCircleOutline />}
          sx={{ alignSelf: "flex-start" }}
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
