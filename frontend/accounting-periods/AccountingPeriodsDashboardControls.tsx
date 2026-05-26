"use client";

import {
  AccountingPeriodSortOrder,
  type AccountingPeriodSortOrder as AccountingPeriodSortOrderType,
} from "@/accounting-periods/types";
import {
  type AccountingPeriodTrendRange,
  accountingPeriodTrendRanges,
} from "@/accounting-periods/dashboard";
import {
  Button,
  InputAdornment,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import type { ChangeEvent, JSX } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Search } from "@mui/icons-material";
import { useDebouncedCallback } from "use-debounce";

/**
 * Props for the AccountingPeriodsDashboardControls component.
 */
interface AccountingPeriodsDashboardControlsProps {
  readonly searchParamName: string;
  readonly sortParamName: string;
  readonly pageParamName: string;
  readonly rangeParamName: string;
  readonly defaultRange: AccountingPeriodTrendRange;
}

/**
 * Sort options available in the Accounting Periods dashboard controls.
 */
const sortOptions: {
  readonly label: string;
  readonly value: AccountingPeriodSortOrderType | "";
}[] = [
  {
    label: "Default order",
    value: "",
  },
  {
    label: "Date: newest first",
    value: AccountingPeriodSortOrder.DateDescending,
  },
  {
    label: "Date: oldest first",
    value: AccountingPeriodSortOrder.Date,
  },
  {
    label: "Open periods first",
    value: AccountingPeriodSortOrder.IsOpenDescending,
  },
  {
    label: "Closed periods first",
    value: AccountingPeriodSortOrder.IsOpen,
  },
  {
    label: "Opening balance: high to low",
    value: AccountingPeriodSortOrder.OpeningBalanceDescending,
  },
  {
    label: "Opening balance: low to high",
    value: AccountingPeriodSortOrder.OpeningBalance,
  },
  {
    label: "Closing balance: high to low",
    value: AccountingPeriodSortOrder.ClosingBalanceDescending,
  },
  {
    label: "Closing balance: low to high",
    value: AccountingPeriodSortOrder.ClosingBalance,
  },
];

/**
 * Renders the hero-band controls for searching, sorting, and changing the trend window.
 */
const AccountingPeriodsDashboardControls = function ({
  searchParamName,
  sortParamName,
  pageParamName,
  rangeParamName,
  defaultRange,
}: AccountingPeriodsDashboardControlsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const currentSearch = searchParams.get(searchParamName) ?? "";
  const currentSort = searchParams.get(sortParamName) ?? "";
  const currentRange = searchParams.get(rangeParamName) ?? String(defaultRange);
  const hasActiveView =
    currentSearch.trim() !== "" ||
    currentSort !== "" ||
    currentRange !== String(defaultRange);

  const handleSearchChange = useDebouncedCallback(
    (event: ChangeEvent<HTMLInputElement>): void => {
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
    event: ChangeEvent<HTMLInputElement>,
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

  const handleRangeChange = function (
    event: ChangeEvent<HTMLInputElement>,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    const nextRange = event.target.value;
    if (nextRange === String(defaultRange)) {
      params.delete(rangeParamName);
    } else {
      params.set(rangeParamName, nextRange);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const clearView = function (): void {
    const params = new URLSearchParams(searchParams.toString());
    params.delete(searchParamName);
    params.delete(sortParamName);
    params.delete(pageParamName);
    params.delete(rangeParamName);
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
          Adjust the registry view and trend window without leaving the page.
        </Typography>
      </Stack>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} useFlexGap>
        <TextField
          fullWidth
          size="small"
          placeholder="Search periods"
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
          sx={{ minWidth: { xs: "100%", sm: 220 } }}
        >
          {sortOptions.map((option) => (
            <MenuItem key={option.label} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          size="small"
          label="Trend range"
          value={currentRange}
          onChange={handleRangeChange}
          sx={{ minWidth: { xs: "100%", sm: 160 } }}
        >
          {accountingPeriodTrendRanges.map((rangeOption) => (
            <MenuItem key={rangeOption} value={String(rangeOption)}>
              Last {rangeOption}
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

export default AccountingPeriodsDashboardControls;
