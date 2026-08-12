"use client";

import { Add, Delete } from "@mui/icons-material";
import {
  Box,
  Button,
  IconButton,
  Stack,
} from "@mui/material";
import type {
  EmployerContributionDraft,
  IncomeDeductionDraft,
  IncomeLineDraft,
  PayrollTaxWithholdingDraft,
} from "@/transactions/workspace/income/helpers";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import Frame from "@/framework/view/Frame";
import { IncomeBreakdownKindModel } from "@/framework/data/api";
import type { JSX } from "react";
import PayrollIncomeDetails from "@/transactions/workspace/income/PayrollIncomeDetails";
import PayrollSectionHeading from "@/transactions/workspace/income/PayrollSectionHeading";
import PayrollTaxWithholdingsSection from "@/transactions/workspace/income/PayrollTaxWithholdingsSection";
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
  income: {
    kind: IncomeBreakdownKindModel.Payroll,
    trackedAmount: null,
    untrackedAmount: null,
    earnings: [],
    employeeDeductions: [],
    employerContributions: [],
    taxWithholdings: [],
  },
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
      {sources.map((source, sourceIndex) => (
        <Stack key={sourceIndex} spacing={3}>
          <Frame
            title="Details"
            color={source.name.trim() === "" ? "error" : "info"}
            headerContent={
              !showSourceControls ? null : (
                <IconButton
                  aria-label="Remove expected income source"
                  color="error"
                  onClick={() => {
                    setSources(
                      sources.filter((_, index) => index !== sourceIndex),
                    );
                  }}
                >
                  <Delete />
                </IconButton>
              )
            }
          >
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
              <StringEntryField
                label="Source Name"
                value={source.name}
                setValue={(name) => {
                  updateSource(sourceIndex, { ...source, name });
                }}
              />
              <StringEntryField
                label="State"
                value={source.income.stateIncomeStateCode ?? null}
                setValue={(stateIncomeStateCode): void => {
                  updateSource(sourceIndex, {
                    ...source,
                    income: { ...source.income, stateIncomeStateCode },
                  });
                }}
              />
            </Box>
          </Frame>
          <Frame
            title="Income Breakdown"
            color="info"
          >
            <PayrollIncomeDetails
              stateIncomeStateCode={source.income.stateIncomeStateCode ?? null}
              setStateIncomeStateCode={null}
              showStateField={false}
              earnings={source.income.earnings as IncomeLineDraft[]}
              setEarnings={(earnings): void => {
                  updateSource(sourceIndex, {
                    ...source,
                    income: {
                      ...source.income,
                      earnings: earnings.map((item) => ({
                        ...item,
                        description: item.description ?? "",
                        amount: item.amount ?? 0,
                      })),
                    },
                  });
                }}
              deductions={source.income.employeeDeductions as IncomeDeductionDraft[]}
              setDeductions={(employeeDeductions): void => {
                  updateSource(sourceIndex, {
                    ...source,
                    income: {
                      ...source.income,
                      employeeDeductions: employeeDeductions.map((item) => ({
                        ...item,
                        description: item.description ?? "",
                        amount: item.amount ?? 0,
                      })),
                    },
                  });
                }}
              contributions={
                  source.income
                    .employerContributions as EmployerContributionDraft[]
                }
              setContributions={(employerContributions): void => {
                  updateSource(sourceIndex, {
                    ...source,
                    income: {
                      ...source.income,
                      employerContributions: employerContributions.map(
                        (item) => ({
                          description: item.description ?? "",
                          amount: item.amount ?? 0,
                        }),
                      ),
                    },
                  });
                }}
              withholdings={source.income.taxWithholdings.map(
                  (item): PayrollTaxWithholdingDraft => ({
                    ...item,
                    jurisdiction: {
                      countryCode: item.jurisdiction.countryCode,
                      subdivisionCode:
                        item.jurisdiction.subdivisionCode ?? null,
                      locality: item.jurisdiction.locality ?? null,
                    },
                  }),
                )}
              setWithholdings={(taxWithholdings): void => {
                  updateSource(sourceIndex, {
                    ...source,
                    income: {
                      ...source.income,
                      taxWithholdings: taxWithholdings.map((item) => ({
                        ...item,
                        amount: item.amount ?? 0,
                        jurisdiction: {
                          countryCode: item.jurisdiction.countryCode ?? "",
                          subdivisionCode: item.jurisdiction.subdivisionCode,
                          locality: item.jurisdiction.locality,
                        },
                      })),
                    },
                  });
                }}
              showWithholdings={false}
            />
            <Stack spacing={1.5} sx={{ mt: 3 }}>
              <PayrollSectionHeading
                title="Expected Payment Dates"
                description="Dates on which this source is expected to pay."
                action={
                  <Button
                    size="small"
                    variant="outlined"
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
                }
              />
              <Box
                sx={{
                display: "grid",
                gap: 1.5,
                gridTemplateColumns: {
                  xs: "1fr",
                  sm: "repeat(auto-fit, minmax(260px, 1fr))",
                },
              }}
              >
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
              </Box>
            </Stack>
          </Frame>
          <Frame title="Withholding" color="info">
            <PayrollTaxWithholdingsSection
              items={source.income.taxWithholdings.map(
                (item): PayrollTaxWithholdingDraft => ({
                  ...item,
                  jurisdiction: {
                    countryCode: item.jurisdiction.countryCode,
                    subdivisionCode: item.jurisdiction.subdivisionCode ?? null,
                    locality: item.jurisdiction.locality ?? null,
                  },
                }),
              )}
              setItems={(taxWithholdings): void => {
                updateSource(sourceIndex, {
                  ...source,
                  income: {
                    ...source.income,
                    taxWithholdings: taxWithholdings.map((item) => ({
                      ...item,
                      amount: item.amount ?? 0,
                      jurisdiction: {
                        countryCode: item.jurisdiction.countryCode ?? "",
                        subdivisionCode: item.jurisdiction.subdivisionCode,
                        locality: item.jurisdiction.locality,
                      },
                    })),
                  },
                });
              }}
            />
          </Frame>
        </Stack>
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
