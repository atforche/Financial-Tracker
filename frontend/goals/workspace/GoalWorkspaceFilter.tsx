"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import {
  type GoalWorkspaceView,
  defaultGoalWorkspaceView,
} from "@/goals/workspace/goalWorkspaceTypes";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";

interface GoalWorkspaceFilterProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly view: GoalWorkspaceView;
}

const normalizeRequestedIds = function (
  values: readonly string[],
): readonly string[] {
  const seenValues = new Set<string>();
  const normalizedValues: string[] = [];

  values.forEach((value) => {
    const nextValue = value.trim();
    if (nextValue === "" || seenValues.has(nextValue)) {
      return;
    }

    seenValues.add(nextValue);
    normalizedValues.push(nextValue);
  });

  return normalizedValues;
};

const normalizeSelectedItems = function <T extends { id: string }>(
  values: readonly string[],
  availableValues: readonly T[],
): readonly T[] {
  const selectedValues = new Set(values);
  if (selectedValues.size === 0 || availableValues.length === 0) {
    return [];
  }

  return availableValues.filter((value) => selectedValues.has(value.id));
};

/**
 * Renders the filter card for the Goal workspace with accounting period and fund filters.
 */
const GoalWorkspaceFilter = function ({
  accountingPeriods,
  funds,
  view,
}: GoalWorkspaceFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const sortParamName = "sort";
  const selectedGoalIdParamName = "selectedGoalId";
  const viewParamName = "view";
  const accountingPeriodParamName = "accountingPeriodIds";
  const fundParamName = "fundIds";

  const currentAccountingPeriods = normalizeSelectedItems(
    normalizeRequestedIds(searchParams.getAll(accountingPeriodParamName)),
    accountingPeriods,
  );
  const currentFunds = normalizeSelectedItems(
    normalizeRequestedIds(searchParams.getAll(fundParamName)),
    funds,
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    params.delete(pageParamName);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

  const hasActiveFilters =
    currentAccountingPeriods.length > 0 || currentFunds.length > 0;

  const handleViewChange = function (nextView: GoalWorkspaceView | null): void {
    if (nextView === null || nextView === view) {
      return;
    }

    updateParams((params) => {
      if (nextView === defaultGoalWorkspaceView) {
        params.delete(viewParamName);
      } else {
        params.set(viewParamName, nextView);
      }
      params.delete(sortParamName);
      params.delete(selectedGoalIdParamName);
    });
  };

  const handleAccountingPeriodChange = function (
    nextAccountingPeriods: readonly AccountingPeriod[],
  ): void {
    updateParams((params) => {
      params.delete(accountingPeriodParamName);
      nextAccountingPeriods.forEach((accountingPeriod) => {
        params.append(accountingPeriodParamName, accountingPeriod.id);
      });
      params.delete(selectedGoalIdParamName);
    });
  };

  const handleFundChange = function (nextFunds: readonly Fund[]): void {
    updateParams((params) => {
      params.delete(fundParamName);
      nextFunds.forEach((fund) => {
        params.append(fundParamName, fund.id);
      });
      params.delete(selectedGoalIdParamName);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(accountingPeriodParamName);
      params.delete(fundParamName);
      params.delete(selectedGoalIdParamName);
    });
  };

  const sharedAutocompleteSx = {
    minWidth: { xs: "100%", sm: 280 },
    flex: { md: 1 },
  };

  return (
    <Paper
      sx={{
        position: "sticky",
        top: 10,
        zIndex: (theme) => theme.zIndex.appBar - 1,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2}>
        <Stack
          direction={{ xs: "column", lg: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "flex-start", lg: "center" }}
        >
          <Stack spacing={0.5}>
            <Typography variant="h5">Goals Workspace</Typography>
          </Stack>
          <ToggleButtonGroup
            exclusive
            size="small"
            value={view}
            onChange={(_, nextValue: GoalWorkspaceView | null) => {
              handleViewChange(nextValue);
            }}
          >
            <ToggleButton value="assignment">Assignment</ToggleButton>
            <ToggleButton value="spending">Spending</ToggleButton>
          </ToggleButtonGroup>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={[...accountingPeriods]}
            value={[...currentAccountingPeriods]}
            disabled={accountingPeriods.length === 0}
            limitTags={1}
            sx={sharedAutocompleteSx}
            noOptionsText={
              accountingPeriods.length === 0
                ? "No accounting periods available"
                : "No accounting periods found"
            }
            isOptionEqualToValue={(option, value) => option.id === value.id}
            slotProps={{
              paper: {
                sx: {
                  "& .MuiAutocomplete-listbox": {
                    maxHeight: 320,
                  },
                },
              },
            }}
            onChange={(_, nextAccountingPeriods) => {
              handleAccountingPeriodChange(nextAccountingPeriods);
            }}
            getOptionLabel={(accountingPeriod) => accountingPeriod.name}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {option.name}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Accounting periods"
                {...(currentAccountingPeriods.length === 0
                  ? { placeholder: "All accounting periods" }
                  : {})}
              />
            )}
          />
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={[...funds]}
            value={[...currentFunds]}
            disabled={funds.length === 0}
            limitTags={1}
            sx={sharedAutocompleteSx}
            noOptionsText={
              funds.length === 0 ? "No funds available" : "No funds found"
            }
            isOptionEqualToValue={(option, value) => option.id === value.id}
            slotProps={{
              paper: {
                sx: {
                  "& .MuiAutocomplete-listbox": {
                    maxHeight: 320,
                  },
                },
              },
            }}
            onChange={(_, nextFunds) => {
              handleFundChange(nextFunds);
            }}
            getOptionLabel={(fund) => fund.name}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {option.name}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Funds"
                {...(currentFunds.length === 0
                  ? { placeholder: "All funds" }
                  : {})}
              />
            )}
          />
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!hasActiveFilters}
            sx={{ flexShrink: 0 }}
          >
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default GoalWorkspaceFilter;
