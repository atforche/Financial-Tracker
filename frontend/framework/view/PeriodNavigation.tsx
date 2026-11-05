"use client";

import { IconButton, Stack, Tooltip } from "@mui/material";
import type { JSX, ReactNode } from "react";
import ChevronLeft from "@mui/icons-material/ChevronLeft";
import ChevronRight from "@mui/icons-material/ChevronRight";
import Link from "next/link";
import type { Route } from "next";

/**
 * A link to an adjacent period.
 */
interface PeriodLink {
  readonly name: string;
  readonly href: Route;
}

/**
 * Links to one adjacent period, disabled rather than removed when there is none so the title doesn't move.
 */
const PeriodArrow = function ({
  period,
  label,
  icon,
}: {
  readonly period: PeriodLink | null;
  readonly label: string;
  readonly icon: ReactNode;
}): JSX.Element {
  return period === null ? (
    <IconButton disabled aria-label={label}>
      {icon}
    </IconButton>
  ) : (
    <Tooltip title={period.name}>
      <IconButton component={Link} href={period.href} aria-label={label}>
        {icon}
      </IconButton>
    </Tooltip>
  );
};

/**
 * Places links to the adjacent accounting periods on either side of the current period's title.
 */
const PeriodNavigation = function ({
  previousPeriod,
  nextPeriod,
  children,
}: {
  readonly previousPeriod: PeriodLink | null;
  readonly nextPeriod: PeriodLink | null;
  readonly children: ReactNode;
}): JSX.Element {
  return (
    <Stack
      direction="row"
      spacing={0.5}
      sx={{ alignItems: "center", minWidth: 0 }}
    >
      <PeriodArrow
        period={previousPeriod}
        label="Previous period"
        icon={<ChevronLeft />}
      />
      {children}
      <PeriodArrow
        period={nextPeriod}
        label="Next period"
        icon={<ChevronRight />}
      />
    </Stack>
  );
};

export default PeriodNavigation;
