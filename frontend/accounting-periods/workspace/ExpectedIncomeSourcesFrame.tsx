"use client";

import { Button, Stack, Typography } from "@mui/material";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import type { JSX } from "react";
import Link from "next/link";
import PeriodProgressRow from "@/accounting-periods/PeriodProgressRow";
import { formatCurrency } from "@/framework/currencyHelpers";
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
 * Shows each expected income source's tracked income received against what it expects, linking to the source.
 */
const ExpectedIncomeSourcesFrame = function ({
  accountingPeriod,
  redirectUrl,
}: ExpectedIncomeSourcesFrameProps): JSX.Element {
  const canManageSources = useWriteAccess() && accountingPeriod.isOpen;
  const sources = accountingPeriod.expectedIncomeSources;
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Stack
          direction="row"
          useFlexGap
          spacing={2}
          sx={{
            flexWrap: "wrap",
            alignItems: "center",
            justifyContent: "space-between",
          }}
        >
          <Typography variant="h5">Expected Income Sources</Typography>
          {canManageSources ? (
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
              caption={[
                `of ${formatCurrency(source.expectedAmount.tracked)} tracked expected`,
                ...source.expectedDates.map((date) =>
                  formatShortDate(new Date(`${date}T00:00:00`)),
                ),
              ].join(" · ")}
            />
          ))
        )}
      </Stack>
    </ContentSurface>
  );
};

export default ExpectedIncomeSourcesFrame;
