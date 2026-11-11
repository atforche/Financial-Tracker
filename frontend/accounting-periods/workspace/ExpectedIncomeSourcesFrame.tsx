"use client";

import { Box, Stack, Typography } from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ActionIconButton from "@/framework/view/ActionIconButton";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import PeriodProgressRow from "@/accounting-periods/PeriodProgressRow";
import { formatShortDate } from "@/framework/dateHelpers";
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
 * Props for the IncomeDifference component.
 */
interface IncomeDifferenceProps {
  readonly received: number;
  readonly expected: number;
}

/**
 * Shows how far the income received is from what was expected, colored as good or bad, or nothing when they match.
 */
const IncomeDifference = function ({
  received,
  expected,
}: IncomeDifferenceProps): JSX.Element | null {
  const difference = getCurrencyDifference(received, expected);
  const comparison = compareCurrencyAmounts(difference, 0);
  if (comparison === 0) {
    return null;
  }
  return (
    <>
      {" · "}
      <Box
        component="span"
        sx={{ color: comparison > 0 ? "success.main" : "error.main" }}
      >
        {comparison > 0
          ? `${formatCurrency(difference)} extra`
          : `${formatCurrency(-difference)} short`}
      </Box>
    </>
  );
};

/**
 * Shows each expected income source's tracked income received against what it expects, linking to the source.
 */
const ExpectedIncomeSourcesFrame = function ({
  accountingPeriod,
  redirectUrl,
}: ExpectedIncomeSourcesFrameProps): JSX.Element {
  const canManageSources = useWriteAccess() && accountingPeriod.isOpen;
  const sources = accountingPeriod.expectedIncomeSources;
  const createUrl = routes.expectedIncomeSourceCreate(
    accountingPeriod.id,
    redirectUrl,
  );
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Stack
          direction="row"
          useFlexGap
          spacing={1}
          sx={{
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h5">Expected Income Sources</Typography>
          {canManageSources ? (
            <ActionIconButton
              action="add"
              label="Add expected income"
              href={createUrl}
            />
          ) : null}
        </Stack>
        {sources.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            Add income sources to plan this period&apos;s expected income.
          </Typography>
        ) : (
          sources.map((source) => (
            <PeriodProgressRow
              key={source.id}
              label={source.name}
              actual={source.receivedAmount.tracked}
              expected={source.expectedAmount.tracked}
              actualLabel="Received"
              expectedLabel="Expected"
              overLabel="Extra income"
              href={routes.expectedIncomeSource(
                accountingPeriod.id,
                source.id,
                redirectUrl,
              )}
              caption={
                <>
                  {`of ${formatCurrency(source.expectedAmount.tracked)} tracked expected`}
                  <IncomeDifference
                    received={source.receivedAmount.tracked}
                    expected={source.expectedAmount.tracked}
                  />
                  {source.expectedDates.length === 0 ? null : (
                    <>
                      <Box
                        component="span"
                        sx={{ display: { xs: "none", sm: "inline" } }}
                      >
                        {" · "}
                      </Box>
                      {/* Phones give the dates their own line, and no date breaks across lines */}
                      <Box
                        component="span"
                        sx={{ display: { xs: "block", sm: "inline" } }}
                      >
                        {source.expectedDates
                          .map((date) =>
                            formatShortDate(
                              new Date(`${date}T00:00:00`),
                            ).replaceAll(" ", "\u00a0"),
                          )
                          .join(" · ")}
                      </Box>
                    </>
                  )}
                </>
              }
            />
          ))
        )}
      </Stack>
    </ContentSurface>
  );
};

export default ExpectedIncomeSourcesFrame;
