"use client";

import {
  Box,
  Button,
  ButtonBase,
  Chip,
  Collapse,
  Menu,
  MenuItem,
  Paper,
  Stack,
  Typography,
  useMediaQuery,
} from "@mui/material";
import { type JSX, useState } from "react";
import {
  compareCurrencyAmounts,
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import ExpandMore from "@mui/icons-material/ExpandMore";
import GoalRangeBar from "@/accounting-periods/workspace/GoalRangeBar";
import Link from "next/link";
import PeriodEntityStatus from "@/accounting-periods/workspace/periodEntityStatus";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import type { Route } from "next";
import SearchBar from "@/framework/view/SearchBar";
import SignedChange from "@/framework/view/SignedChange";
import parseEnumValue from "@/framework/data/parseEnumValue";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";
import { useTheme } from "@mui/material/styles";

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
  readonly isIncreaseGood: boolean;
  readonly contribution: {
    readonly assigned: number;
    readonly expected: number;
    readonly isSatisfied: boolean;
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
    status: PeriodEntityStatus.NotFullyFunded,
    label: "Not fully funded",
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
 * Sections that start collapsed on phones, leaving the ones that need attention open.
 */
const closedOnPhone: ReadonlySet<PeriodEntityStatus> = new Set([
  PeriodEntityStatus.WithinRange,
  PeriodEntityStatus.NoGoal,
]);

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
 * Determines whether an entity is short of its expected contribution.
 */
const isNotFullyFunded = function ({ contribution }: PeriodEntity): boolean {
  return contribution !== null && !contribution.isSatisfied;
};

/**
 * Gets the status section an entity belongs to. Range problems take priority, so only an entity that would otherwise
 * read as in range moves to the not fully funded section.
 */
const getSectionStatus = function (entity: PeriodEntity): PeriodEntityStatus {
  return entity.status === PeriodEntityStatus.WithinRange &&
    isNotFullyFunded(entity)
    ? PeriodEntityStatus.NotFullyFunded
    : entity.status;
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
 * Hides a tile detail on phones, where the list of tiles runs long.
 */
const phoneHiddenSx = { display: { xs: "none", sm: "block" } } as const;

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
        gap: { xs: 0.5, sm: 0.75 },
        p: 1.5,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        color: "inherit",
        textDecoration: "none",
        "&:hover": { borderColor: "primary.main" },
      }}
    >
      <Stack
        direction="row"
        spacing={1}
        sx={{ alignItems: "center", justifyContent: "space-between" }}
      >
        <Typography variant="subtitle2" noWrap>
          {entity.name}
        </Typography>
        {isNotFullyFunded(entity) ? (
          <Chip
            label="Not fully funded"
            size="small"
            color="warning"
            variant="outlined"
          />
        ) : null}
      </Stack>
      <Stack
        direction="row"
        spacing={2}
        sx={{ alignItems: "baseline", justifyContent: "space-between" }}
      >
        <Box sx={{ minWidth: 0 }}>
          <Typography
            variant="caption"
            color="text.secondary"
            component="p"
            sx={phoneHiddenSx}
          >
            Closing balance
          </Typography>
          <Typography
            variant="h6"
            component="p"
            sx={{ fontWeight: 500, fontSize: { xs: "1.1rem", sm: undefined } }}
          >
            {formatCurrency(entity.closingBalance)}
          </Typography>
        </Box>
        <Box sx={{ textAlign: "right", whiteSpace: "nowrap" }}>
          <Typography
            variant="caption"
            color="text.secondary"
            component="p"
            sx={phoneHiddenSx}
          >
            This period
          </Typography>
          <Typography variant="body2" component="p">
            <SignedChange
              amount={getChange(entity)}
              isIncreaseGood={entity.isIncreaseGood}
            />
          </Typography>
        </Box>
      </Stack>
      <GoalRangeBar
        minimum={entity.minimum}
        maximum={entity.maximum}
        balance={entity.closingBalance}
        color={color}
      />
      <Typography
        variant="caption"
        color="text.secondary"
        // Phones leave out "No goal", since the missing bar already says so
        sx={entity.minimum === null ? phoneHiddenSx : {}}
      >
        {getGoalLabel(entity)}
      </Typography>
      {entity.contribution === null ||
      (entity.contribution.isSatisfied &&
        compareCurrencyAmounts(entity.contribution.expected, 0) ===
          0) ? null : (
        <Typography
          variant="caption"
          color={isNotFullyFunded(entity) ? "warning.main" : "text.secondary"}
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
 * Searches and sorts the accounts or funds an AccountingPeriodEntityList shows, through the URL that it reads.
 */
const AccountingPeriodEntityControls = function (): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater([]);
  const [sortAnchor, setSortAnchor] = useState<HTMLElement | null>(null);
  const sort =
    parseEnumValue(EntitySort, searchParams.get("entitySort") ?? "") ??
    EntitySort.Status;
  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
      <Box
        sx={{ flex: { xs: 1, sm: "none" }, width: { sm: 220 }, minWidth: 0 }}
      >
        <SearchBar searchParamName="search" />
      </Box>
      <Button
        color="inherit"
        endIcon={<ExpandMore />}
        onClick={(event) => {
          setSortAnchor(event.currentTarget);
        }}
        sx={{ flexShrink: 0, textTransform: "none" }}
      >
        Sort: {sortLabels[sort]}
      </Button>
      <Menu
        anchorEl={sortAnchor}
        open={sortAnchor !== null}
        onClose={() => {
          setSortAnchor(null);
        }}
      >
        {Object.values(EntitySort).map((value) => (
          <MenuItem
            key={value}
            selected={value === sort}
            onClick={() => {
              updateParams((params) => {
                params.set("entitySort", value);
              });
              setSortAnchor(null);
            }}
          >
            {sortLabels[value]}
          </MenuItem>
        ))}
      </Menu>
    </Stack>
  );
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
  const sort =
    parseEnumValue(EntitySort, searchParams.get("entitySort") ?? "") ??
    EntitySort.Status;
  const search = (searchParams.get("search") ?? "").trim().toLowerCase();
  const theme = useTheme();
  const isPhone = useMediaQuery(theme.breakpoints.down("sm"));
  const [openStatuses, setOpenStatuses] = useState<
    Partial<Record<PeriodEntityStatus, boolean>>
  >({});
  const visible = entities
    .filter((entity) => entity.name.toLowerCase().includes(search))
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
              statuses.find(({ status }) => status === getSectionStatus(entity))
                ?.color ?? "text.secondary"
            }
          />
        ))}
      </ResponsiveGrid>
    );
  };
  const sections = statuses
    .map((details) => ({
      ...details,
      tiles: visible.filter(
        (entity) => getSectionStatus(entity) === details.status,
      ),
    }))
    .filter(({ tiles }) => tiles.length > 0);
  // A search opens every section so no match is hidden
  const isOpen = function (status: PeriodEntityStatus): boolean {
    return (
      search !== "" || (openStatuses[status] ?? !closedOnPhone.has(status))
    );
  };

  return (
    <Stack spacing={2.5}>
      {visible.length === 0 ? (
        <Typography color="text.secondary">
          Nothing matches the search.
        </Typography>
      ) : null}
      {sort === EntitySort.Status
        ? sections.map(({ status, label, color, tiles }) => (
            <Stack key={status} spacing={1.25}>
              <ButtonBase
                aria-expanded={isOpen(status)}
                onClick={() => {
                  setOpenStatuses((current) => ({
                    ...current,
                    [status]: !isOpen(status),
                  }));
                }}
                // Only phones collapse sections, so wider screens show a plain heading
                disabled={!isPhone}
                sx={{ justifyContent: "space-between", borderRadius: 1 }}
              >
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
                {isPhone ? (
                  <ExpandMore
                    color="action"
                    sx={{
                      transform: isOpen(status) ? "rotate(180deg)" : "none",
                      transition: "transform 0.2s ease-in-out",
                    }}
                  />
                ) : null}
              </ButtonBase>
              <Collapse in={!isPhone || isOpen(status)}>
                {renderTiles(tiles)}
              </Collapse>
            </Stack>
          ))
        : renderTiles(visible)}
    </Stack>
  );
};

export { AccountingPeriodEntityControls };
export default AccountingPeriodEntityList;
