"use client";

import { Box, Stack, Typography } from "@mui/material";
import type { JSX, ReactNode } from "react";
import Frame from "@/framework/view/Frame";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";
import { formatCurrency } from "@/framework/currencyHelpers";

interface ExpectedIncomeTotalsFrameProps {
  readonly headerContent?: ReactNode;
  readonly sourceName: string;
  readonly accountingPeriodName: string;
  readonly setSourceName?: ((name: string) => void) | null;
  readonly netPerPayment: number;
  readonly trackedPerPayment: number;
  readonly untrackedPerPayment: number;
  readonly paymentCount: number;
}

/** Shows expected income and its tracked/untracked composition at either scale. */
const ExpectedIncomeTotalsFrame = function ({
  headerContent,
  sourceName,
  accountingPeriodName,
  setSourceName = null,
  netPerPayment,
  trackedPerPayment,
  untrackedPerPayment,
  paymentCount,
}: ExpectedIncomeTotalsFrameProps): JSX.Element {
  const total = netPerPayment * paymentCount;
  const tracked = trackedPerPayment * paymentCount;
  const untracked = untrackedPerPayment * paymentCount;
  const validBreakdown =
    total >= 0 &&
    tracked >= 0 &&
    untracked >= 0 &&
    Math.abs(tracked + untracked - total) < 0.005;
  const trackedPercent =
    validBreakdown && total > 0 ? (tracked / total) * 100 : 0;
  const untrackedPercent =
    validBreakdown && total > 0 ? (untracked / total) * 100 : 0;
  const renderBar = (
    label: string,
    amount: number,
    amountColor: string,
    content: JSX.Element,
    rightLabel?: JSX.Element,
  ): JSX.Element => (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        justifyContent="space-between"
        spacing={1}
        flexWrap="wrap"
        useFlexGap
      >
        <Typography variant="body2" fontWeight={600} color={amountColor} noWrap>
          {label}: {formatCurrency(amount)}
        </Typography>
        {rightLabel}
      </Stack>
      {content}
    </Stack>
  );

  const renderBarBackground = (content: JSX.Element): JSX.Element => (
    <Box
      role="img"
      sx={{
        display: "flex",
        width: "100%",
        height: 16,
        borderRadius: 1,
        backgroundColor: "divider",
        overflow: "hidden",
      }}
    >
      {content}
    </Box>
  );

  return (
    <Frame title="Summary" color="info" headerContent={headerContent}>
      <Stack spacing={1.5} sx={{ px: { xs: 1, sm: 2 }, py: 1 }}>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <StringEntryField
            label="Source Name"
            value={sourceName}
            setValue={setSourceName}
          />
          <StringEntryField
            label="Accounting Period"
            value={accountingPeriodName}
            setValue={null}
          />
        </ResponsiveGrid>
        {renderBar(
          "Expected",
          total,
          "info.main",
          renderBarBackground(
            <Box
              sx={{ width: total > 0 ? "100%" : 0, bgcolor: "info.main" }}
            />,
          ),
        )}
        {renderBar(
          "Tracked",
          tracked,
          "success.main",
          renderBarBackground(
            <>
              <Box
                sx={{ width: `${trackedPercent}%`, bgcolor: "success.main" }}
              />
              <Box
                sx={{
                  width: `${untrackedPercent}%`,
                  bgcolor: "warning.main",
                }}
              />
            </>,
          ),
          <Typography
            variant="body2"
            fontWeight={600}
            color="warning.main"
            textAlign="right"
            noWrap
          >
            Untracked: {formatCurrency(untracked)}
          </Typography>,
        )}
      </Stack>
    </Frame>
  );
};

export default ExpectedIncomeTotalsFrame;
