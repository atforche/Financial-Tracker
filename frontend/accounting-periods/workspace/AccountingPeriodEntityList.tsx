"use client";

import {
  Box,
  ButtonBase,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import type { JSX } from "react";
import Link from "next/link";
import PeriodEntityStatus from "@/accounting-periods/workspace/periodEntityStatus";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import type { Route } from "next";
import SearchBar from "@/framework/listframe/SearchBar";
import SignedChange from "@/framework/view/SignedChange";
import parseEnumValue from "@/framework/data/parseEnumValue";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * An account or fund's balances and goal status in an accounting period.
 */
interface PeriodEntity {
  readonly id: string;
  readonly name: string;
  readonly href: Route;
  readonly openingBalance: number;
  readonly closingBalance: number;
  readonly minimum: number | null;
  readonly maximum: number | null;
  readonly status: PeriodEntityStatus;
  readonly contribution: {
    readonly assigned: number;
    readonly expected: number;
  } | null;
}

/**
 * Statuses in the order their sections appear, with their labels and colors.
 */
const statuses = [
  {
    status: PeriodEntityStatus.BelowMinimum,
    label: "Below minimum",
    color: "error.main",
  },
  {
    status: PeriodEntityStatus.AboveMaximum,
    label: "Above maximum",
    color: "warning.main",
  },
  {
    status: PeriodEntityStatus.WithinRange,
    label: "In range",
    color: "success.main",
  },
  {
    status: PeriodEntityStatus.NoGoal,
    label: "No goal",
    color: "text.secondary",
  },
] as const;

/**
 * Supported tile orders within each section.
 */
enum EntitySort {
  Status = "Status",
  Name = "Name",
  Closing = "Closing",
  Change = "Change",
}

const sortLabels: Record<EntitySort, string> = {
  [EntitySort.Status]: "Status",
  [EntitySort.Name]: "Name",
  [EntitySort.Closing]: "Closing balance",
  [EntitySort.Change]: "Change",
};

/**
 * Gets how much an entity's balance changed over the period.
 */
const getChange = function (entity: PeriodEntity): number {
  return getCurrencyDifference(entity.closingBalance, entity.openingBalance);
};

/**
 * Gets how far an entity ended outside its goal range.
 */
const getDistance = function ({
  closingBalance,
  minimum,
  maximum,
  status,
}: PeriodEntity): number {
  if (status === PeriodEntityStatus.BelowMinimum && minimum !== null) {
    return getCurrencyDifference(minimum, closingBalance);
  }
  return status === PeriodEntityStatus.AboveMaximum && maximum !== null
    ? getCurrencyDifference(closingBalance, maximum)
    : 0;
};

/**
 * Describes an entity's goal for the period.
 */
const getGoalLabel = function ({ minimum, maximum }: PeriodEntity): string {
  if (minimum === null) {
    return "No goal";
  }
  if (maximum === null) {
    return `Goal: at least ${formatCurrency(minimum)}`;
  }
  return compareCurrencyAmounts(minimum, maximum) === 0
    ? `Goal: exactly ${formatCurrency(minimum)}`
    : `Goal: ${formatCurrency(minimum)} to ${formatCurrency(maximum)}`;
};

/**
 * Shows a closing balance against its goal range on one compact scale.
 */
const GoalRangeBar = function ({
  entity,
  color,
}: {
  readonly entity: PeriodEntity;
  readonly color: string;
}): JSX.Element | null {
  const { minimum, maximum, closingBalance } = entity;
  if (minimum === null) {
    return null;
  }
  const upperBound = maximum ?? minimum;
  const lower = Math.min(closingBalance, minimum);
  const upper = Math.max(closingBalance, upperBound);
  const padding = Math.max(upper - lower, Math.abs(minimum), 1) * 0.15;
  const position = (amount: number): number =>
    ((amount - lower + padding) / (upper - lower + padding * 2)) * 100;
  return (
    <Box sx={{ position: "relative", height: 14 }}>
      <Box
        sx={{
          position: "absolute",
          inset: "3px 0",
          borderRadius: 999,
          bgcolor: "action.hover",
        }}
      />
      <Box
        sx={{
          position: "absolute",
          top: 3,
          bottom: 3,
          left: `${position(minimum)}%`,
          width: `${(maximum === null ? 100 : position(maximum)) - position(minimum)}%`,
          borderRadius: 999,
          bgcolor: "success.main",
          opacity: 0.35,
        }}
      />
      <Box
        sx={{
          position: "absolute",
          left: `${position(closingBalance)}%`,
          width: 14,
          height: 14,
          borderRadius: "50%",
          border: "2px solid",
          borderColor: "background.paper",
          bgcolor: color,
          transform: "translateX(-50%)",
        }}
      />
    </Box>
  );
};

/**
 * Links to an account or fund's goal, summarizing how it ended the period.
 */
const EntityTile = function ({
  entity,
  color,
}: {
  readonly entity: PeriodEntity;
  readonly color: string;
}): JSX.Element {
  const distance = getDistance(entity);
  const isExactTarget =
    entity.minimum !== null &&
    entity.maximum !== null &&
    compareCurrencyAmounts(entity.minimum, entity.maximum) === 0;
  return (
    <Paper
      component={Link}
      href={entity.href}
      sx={{
        display: "flex",
        flexDirection: "column",
        gap: 0.75,
        p: 1.5,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        color: "inherit",
        textDecoration: "none",
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <Typography variant="subtitle2" noWrap>
        {entity.name}
      </Typography>
      <Stack
        direction="row"
        spacing={2}
        sx={{ alignItems: "flex-start", justifyContent: "space-between" }}
      >
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="caption" color="text.secondary" component="p">
            Closing balance
          </Typography>
          <Typography variant="h6" component="p" sx={{ fontWeight: 500 }}>
            {formatCurrency(entity.closingBalance)}
          </Typography>
        </Box>
        <Box sx={{ textAlign: "right", whiteSpace: "nowrap" }}>
          <Typography variant="caption" color="text.secondary" component="p">
            This period
          </Typography>
          <Typography variant="body2" component="p">
            <SignedChange amount={getChange(entity)} />
          </Typography>
        </Box>
      </Stack>
      <GoalRangeBar entity={entity} color={color} />
      <Typography variant="caption" color="text.secondary">
        {getGoalLabel(entity)}
      </Typography>
      {entity.contribution === null ||
      compareCurrencyAmounts(entity.contribution.expected, 0) === 0 ? null : (
        <Typography
          variant="caption"
          color={
            compareCurrencyAmounts(
              entity.contribution.assigned,
              entity.contribution.expected,
            ) < 0
              ? "warning.main"
              : "text.secondary"
          }
        >
          {formatCurrency(entity.contribution.assigned)} of{" "}
          {formatCurrency(entity.contribution.expected)} contributed
        </Typography>
      )}
      {distance === 0 ? null : (
        <Typography variant="caption" sx={{ color, fontWeight: 500 }}>
          {formatCurrency(distance)}{" "}
          {entity.status === PeriodEntityStatus.BelowMinimum
            ? `below ${isExactTarget ? "target" : "minimum"}`
            : `above ${isExactTarget ? "target" : "maximum"}`}
        </Typography>
      )}
    </Paper>
  );
};

/**
 * Compares entities for the selected sort.
 */
const compareEntities = function (
  first: PeriodEntity,
  second: PeriodEntity,
  sort: EntitySort,
): number {
  switch (sort) {
    case EntitySort.Name:
      return 0;
    case EntitySort.Closing:
      return second.closingBalance - first.closingBalance;
    case EntitySort.Change:
      return getChange(second) - getChange(first);
    case EntitySort.Status:
    default:
      return getDistance(second) - getDistance(first);
  }
};

/**
 * Compares accounts or funds within one accounting period as tiles, grouped by goal status with problems first
 * unless another sort is chosen. Each tile links to the account or fund's goal for that period.
 */
const AccountingPeriodEntityList = function ({
  entities,
}: {
  readonly entities: readonly PeriodEntity[];
}): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const sort =
    parseEnumValue(EntitySort, searchParams.get("entitySort") ?? "") ??
    EntitySort.Status;
  const selectedStatus = parseEnumValue(
    PeriodEntityStatus,
    searchParams.get("status") ?? "",
  );
  const search = (searchParams.get("search") ?? "").trim().toLowerCase();
  const presentStatuses = statuses.filter(({ status }) =>
    entities.some((entity) => entity.status === status),
  );
  const setParam = function (name: string, value: string | null): void {
    updateParams((params) => {
      if (value === null) {
        params.delete(name);
      } else {
        params.set(name, value);
      }
    });
  };
  const visible = entities
    .filter(
      (entity) =>
        (selectedStatus === null || entity.status === selectedStatus) &&
        entity.name.toLowerCase().includes(search),
    )
    .sort(
      (first, second) =>
        compareEntities(first, second, sort) ||
        first.name.localeCompare(second.name),
    );
  const renderTiles = function (tiles: readonly PeriodEntity[]): JSX.Element {
    return (
      <ResponsiveGrid columns={{ xs: 1, sm: 2, md: 3, lg: 4 }} spacing={1.5}>
        {tiles.map((entity) => (
          <EntityTile
            key={entity.id}
            entity={entity}
            color={
              statuses.find(({ status }) => status === entity.status)?.color ??
              "text.secondary"
            }
          />
        ))}
      </ResponsiveGrid>
    );
  };
  const sections = presentStatuses
    .map((details) => ({
      ...details,
      tiles: visible.filter((entity) => entity.status === details.status),
    }))
    .filter(({ tiles }) => tiles.length > 0);

  return (
    <Stack spacing={2.5}>
      <Stack
        direction="row"
        useFlexGap
        spacing={1}
        sx={{ alignItems: "center", flexWrap: "wrap" }}
      >
        {presentStatuses.map(({ status, label, color }) => (
          <ButtonBase
            key={status}
            aria-pressed={selectedStatus === status}
            onClick={() => {
              setParam("status", selectedStatus === status ? null : status);
            }}
            sx={{
              gap: 1,
              minHeight: 40,
              px: 1.5,
              border: "1px solid",
              borderColor: "divider",
              borderRadius: 2,
              bgcolor:
                selectedStatus === status ? "action.selected" : "transparent",
              "&:hover": { bgcolor: "action.hover" },
            }}
          >
            <Box
              sx={{
                width: 10,
                height: 10,
                borderRadius: "50%",
                bgcolor: color,
              }}
            />
            <Typography variant="body2" sx={{ fontWeight: 500 }}>
              {entities
                .filter((entity) => entity.status === status)
                .length.toString()}{" "}
              {label}
            </Typography>
          </ButtonBase>
        ))}
        <Box sx={{ flex: 1, minWidth: 220 }}>
          <SearchBar searchParamName="search" />
        </Box>
        <TextField
          select
          size="small"
          label="Sort by"
          value={sort}
          onChange={(event) => {
            setParam("entitySort", event.target.value);
          }}
          sx={{ minWidth: 190 }}
        >
          {Object.values(EntitySort).map((value) => (
            <MenuItem key={value} value={value}>
              {sortLabels[value]}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
      {visible.length === 0 ? (
        <Typography color="text.secondary">
          Nothing matches the search or status filter.
        </Typography>
      ) : null}
      {sort === EntitySort.Status
        ? sections.map(({ status, label, color, tiles }) => (
            <Stack key={status} spacing={1.25}>
              <Typography variant="subtitle1" sx={{ fontWeight: 500, color }}>
                {label}{" "}
                <Typography
                  component="span"
                  variant="body2"
                  color="text.secondary"
                >
                  {tiles.length}
                </Typography>
              </Typography>
              {renderTiles(tiles)}
            </Stack>
          ))
        : renderTiles(visible)}
    </Stack>
  );
};

export default AccountingPeriodEntityList;
