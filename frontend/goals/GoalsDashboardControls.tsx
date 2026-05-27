"use client";

import {
  AccountingPeriodGoalSortOrder,
  type AccountingPeriodIdentifier,
} from "@/accounting-periods/types";
import {
  Button,
  InputAdornment,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { JSX } from "react";
import { Search } from "@mui/icons-material";
import { useDebouncedCallback } from "use-debounce";

/**
 * Props for the GoalsDashboardControls component.
 */
interface GoalsDashboardControlsProps {
  readonly accountingPeriods: AccountingPeriodIdentifier[];
  readonly accountingPeriodParamName: string;
  readonly searchParamName: string;
  readonly sortParamName: string;
  readonly pageParamName: string;
}

/**
 * Sort options available in the Goals dashboard controls.
 */
const sortOptions: {
  readonly label: string;
  readonly value: AccountingPeriodGoalSortOrder | "";
}[] = [
  {
    label: "Default order",
    value: "",
  },
  {
    label: "Fund: A to Z",
    value: AccountingPeriodGoalSortOrder.Name,
  },
  {
    label: "Fund: Z to A",
    value: AccountingPeriodGoalSortOrder.NameDescending,
  },
  {
    label: "Goal type: A to Z",
    value: AccountingPeriodGoalSortOrder.Type,
  },
  {
    label: "Goal type: Z to A",
    value: AccountingPeriodGoalSortOrder.TypeDescending,
  },
  {
    label: "Goal amount: low to high",
    value: AccountingPeriodGoalSortOrder.GoalAmount,
  },
  {
    label: "Goal amount: high to low",
    value: AccountingPeriodGoalSortOrder.GoalAmountDescending,
  },
  {
    label: "Remaining to assign: low to high",
    value: AccountingPeriodGoalSortOrder.RemainingAmountToAssign,
  },
  {
    label: "Remaining to assign: high to low",
    value: AccountingPeriodGoalSortOrder.RemainingAmountToAssignDescending,
  },
  {
    label: "Remaining to spend: low to high",
    value: AccountingPeriodGoalSortOrder.RemainingAmountToSpend,
  },
  {
    label: "Remaining to spend: high to low",
    value: AccountingPeriodGoalSortOrder.RemainingAmountToSpendDescending,
  },
];

/**
 * Renders the hero-band controls for selecting a period, filtering, and sorting the Goals view.
 */
const GoalsDashboardControls = function ({
  accountingPeriods,
  accountingPeriodParamName,
  searchParamName,
  sortParamName,
  pageParamName,
}: GoalsDashboardControlsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const currentAccountingPeriodId =
    searchParams.get(accountingPeriodParamName) ?? "";
  const currentSearch = searchParams.get(searchParamName) ?? "";
  const currentSort = searchParams.get(sortParamName) ?? "";
  const hasActiveView =
    currentAccountingPeriodId !== "" ||
    currentSearch.trim() !== "" ||
    currentSort !== "";

  const handleSearchChange = useDebouncedCallback(
    (event: React.ChangeEvent<HTMLInputElement>): void => {
      const params = new URLSearchParams(searchParams.toString());
      const { value } = event.target;
      if (value.trim() !== "") {
        params.set(searchParamName, value);
      } else {
        params.delete(searchParamName);
      }
      params.delete(pageParamName);
      router.replace(`${pathname}?${params.toString()}`);
    },
    300,
  );

  const handleSortChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    const nextSort = event.target.value;
    if (nextSort === "") {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, nextSort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const handleAccountingPeriodChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    const nextAccountingPeriodId = event.target.value;
    if (nextAccountingPeriodId === "") {
      params.delete(accountingPeriodParamName);
    } else {
      params.set(accountingPeriodParamName, nextAccountingPeriodId);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const clearView = function (): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(accountingPeriodParamName);
    params.delete(searchParamName);
    params.delete(sortParamName);
    params.delete(pageParamName);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`);
  };

  return (
    <Stack
      spacing={1.5}
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        bgcolor: "background.paper",
        p: 2,
      }}
    >
      <Stack spacing={0.5}>
        <Typography variant="overline" color="text.secondary">
          Workspace controls
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Switch periods, search by fund or goal type, and change the table
          order without leaving the workspace.
        </Typography>
      </Stack>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} useFlexGap>
        <TextField
          select
          size="small"
          label="Accounting period"
          value={currentAccountingPeriodId}
          onChange={handleAccountingPeriodChange}
          sx={{ minWidth: { xs: "100%", sm: 220 } }}
        >
          <MenuItem value="">Current or latest period</MenuItem>
          {accountingPeriods.map((accountingPeriod) => (
            <MenuItem key={accountingPeriod.id} value={accountingPeriod.id}>
              {accountingPeriod.name}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          fullWidth
          size="small"
          placeholder="Search goals"
          defaultValue={currentSearch}
          onChange={handleSearchChange}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search />
                </InputAdornment>
              ),
            },
          }}
        />
        <TextField
          select
          size="small"
          label="Sort"
          value={currentSort}
          onChange={handleSortChange}
          sx={{ minWidth: { xs: "100%", sm: 250 } }}
        >
          {sortOptions.map((option) => (
            <MenuItem key={option.label} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
        <Button
          variant="outlined"
          onClick={clearView}
          disabled={!hasActiveView}
          sx={{ flexShrink: 0 }}
        >
          Clear view
        </Button>
      </Stack>
    </Stack>
  );
};

export default GoalsDashboardControls;
