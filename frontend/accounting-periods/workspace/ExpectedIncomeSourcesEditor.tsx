"use client";

import { Box, Divider, Stack, Typography } from "@mui/material";
import CollectionEditor from "@/framework/view/CollectionEditor";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import Frame from "@/framework/view/Frame";
import InsetFrame from "@/framework/view/InsetFrame";
import type { JSX } from "react";
import StringEntryField from "@/framework/forms/StringEntryField";
import { alpha } from "@mui/material/styles";
import dayjs from "dayjs";

/**
 * Props for the ExpectedIncomeSourcesEditor component.
 */
interface ExpectedIncomeSourcesEditorProps {
  readonly source: ExpectedIncomeSourceRequest;
  readonly setSource: (source: ExpectedIncomeSourceRequest) => void;
  readonly year: number | null;
  readonly month: number | null;
}

/**
 * Edits one expected-income source for an Accounting Period.
 */
const ExpectedIncomeSourcesEditor = function ({
  source,
  setSource,
  year,
  month,
}: ExpectedIncomeSourcesEditorProps): JSX.Element {
  const minDate =
    year === null || month === null
      ? null
      : dayjs(new Date(year, month - 1, 1));
  const maxDate = minDate?.endOf("month") ?? null;

  return (
    <Frame title={source.name || "Expected Income Source"} color="info">
      <Stack spacing={1.5}>
        <Stack direction="row" spacing={1} alignItems="center">
          <StringEntryField
            label="Source Name"
            value={source.name}
            setValue={(name) => {
              setSource({ ...source, name });
            }}
          />
        </Stack>
        <Divider />
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">Income Lines</Typography>
          <Typography variant="body2" color="text.secondary">
            Add the gross income amounts that make up each expected payment.
          </Typography>
        </Stack>
        <CollectionEditor
          items={source.incomeLines}
          setItems={(incomeLines) => {
            setSource({ ...source, incomeLines });
          }}
          createItem={() => ({ description: "", amount: 0 })}
          addLabel="Add Income Line"
          canDeleteItem={(_, __, items) => items.length > 1}
          itemContainerSx={{
            display: "grid",
            gap: 1.5,
            gridTemplateColumns: {
              xs: "1fr",
              md: "repeat(2, minmax(0, 1fr))",
            },
          }}
          renderItem={(line, lineIndex, itemControls) => (
            <InsetFrame
              key={lineIndex}
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: {
                  xs: "1fr",
                  md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
                },
                alignItems: "start",
              }}
            >
              <StringEntryField
                label="Description"
                value={line.description}
                autoFocus={itemControls.autoFocus}
                setValue={(description) => {
                  const incomeLines = source.incomeLines.map((item, index) =>
                    index === lineIndex ? { ...item, description } : item,
                  );
                  setSource({ ...source, incomeLines });
                }}
              />
              <CurrencyEntryField
                label="Amount"
                value={line.amount}
                setValue={(amount) => {
                  const incomeLines = source.incomeLines.map((item, index) =>
                    index === lineIndex
                      ? { ...item, amount: amount ?? 0 }
                      : item,
                  );
                  setSource({ ...source, incomeLines });
                }}
              />
              <Box
                sx={{
                  display: "flex",
                  justifyContent: { xs: "flex-end", md: "center" },
                  pt: { xs: 0, md: 1.25 },
                }}
              >
                {itemControls.deleteButton}
              </Box>
            </InsetFrame>
          )}
        />
        <Divider />
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">Deductions</Typography>
          <Typography variant="body2" color="text.secondary">
            Add optional deductions withheld before the income is deposited.
          </Typography>
        </Stack>
        <CollectionEditor
          items={source.incomeDeductions}
          setItems={(incomeDeductions) => {
            setSource({ ...source, incomeDeductions });
          }}
          createItem={() => ({ description: "", amount: 0 })}
          addLabel="Add Deduction"
          itemContainerSx={{
            display: "grid",
            gap: 1.5,
            gridTemplateColumns: {
              xs: "1fr",
              md: "repeat(2, minmax(0, 1fr))",
            },
          }}
          renderItem={(deduction, deductionIndex, itemControls) => (
            <InsetFrame
              key={deductionIndex}
              sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: {
                  xs: "1fr",
                  md: "minmax(0, 1.8fr) minmax(180px, 1fr) auto",
                },
                alignItems: "start",
              }}
            >
              <StringEntryField
                label="Description"
                value={deduction.description}
                autoFocus={itemControls.autoFocus}
                setValue={(description) => {
                  const incomeDeductions = source.incomeDeductions.map(
                    (item, index) =>
                      index === deductionIndex
                        ? { ...item, description }
                        : item,
                  );
                  setSource({
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
                  setSource({
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
                {itemControls.deleteButton}
              </Box>
            </InsetFrame>
          )}
        />
        <Divider />
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">Untracked Transfers</Typography>
          <Typography variant="body2" color="text.secondary">
            Add amounts expected to remain outside tracked accounts.
          </Typography>
        </Stack>
        <CollectionEditor
          items={source.untrackedTransfers}
          setItems={(untrackedTransfers) => {
            setSource({ ...source, untrackedTransfers });
          }}
          createItem={() => ({ description: "", amount: 0 })}
          addLabel="Add Untracked Transfer"
          itemContainerSx={{
            display: "grid",
            gap: 1.5,
            gridTemplateColumns: {
              xs: "1fr",
              md: "repeat(2, minmax(0, 1fr))",
            },
          }}
          renderItem={(transfer, transferIndex, itemControls) => (
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
                autoFocus={itemControls.autoFocus}
                setValue={(description) => {
                  const untrackedTransfers = source.untrackedTransfers.map(
                    (item, index) =>
                      index === transferIndex ? { ...item, description } : item,
                  );
                  setSource({
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
                  setSource({
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
                {itemControls.deleteButton}
              </Box>
            </Box>
          )}
        />
        <Divider />
        <Stack spacing={0.25}>
          <Typography variant="subtitle2">Expected Payment Dates</Typography>
          <Typography variant="body2" color="text.secondary">
            Add the dates on which this source is expected to pay.
          </Typography>
        </Stack>
        <CollectionEditor
          items={source.expectedDates}
          setItems={(expectedDates) => {
            setSource({ ...source, expectedDates });
          }}
          createItem={() => minDate?.format("YYYY-MM-DD") ?? ""}
          addLabel="Add Expected Date"
          showAddButton={minDate !== null}
          itemContainerSx={{
            display: "grid",
            gap: 1.5,
            gridTemplateColumns: "repeat(auto-fit, minmax(260px, 1fr))",
          }}
          renderItem={(date, dateIndex, itemControls) => (
            <Box
              key={dateIndex}
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
                autoFocus={itemControls.autoFocus}
                setValue={(value) => {
                  if (value === null) {
                    setSource({
                      ...source,
                      expectedDates: source.expectedDates.filter(
                        (_, index) => index !== dateIndex,
                      ),
                    });
                    return;
                  }
                  if (!value.isValid()) {
                    return;
                  }
                  const expectedDates = source.expectedDates.map(
                    (item, index) =>
                      index === dateIndex ? value.format("YYYY-MM-DD") : item,
                  );
                  setSource({ ...source, expectedDates });
                }}
              />
              <Box
                sx={{
                  display: "flex",
                  justifyContent: { xs: "flex-end", md: "center" },
                  pt: { xs: 0, md: 1.25 },
                }}
              >
                {itemControls.deleteButton}
              </Box>
            </Box>
          )}
        />
      </Stack>
    </Frame>
  );
};

export default ExpectedIncomeSourcesEditor;
