"use client";

import {
  AccountDashboardSortOrder,
  AccountType,
  formatAccountType,
} from "@/accounts/types";
import {
  Button,
  InputAdornment,
  MenuItem,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import { Search } from "@mui/icons-material";
import { useDebouncedCallback } from "use-debounce";

/**
 * Dashboard filter mode values used in the Accounts view URL.
 */
type AccountsDashboardFilterMode = "accounting-period" | "date";

/**
 * Props for the AccountsDashboardControls component.
 */
interface AccountsDashboardControlsProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Sort options available in the Accounts dashboard controls.
 */
const sortOptions: {
  readonly label: string;
  readonly value: AccountDashboardSortOrder | "";
}[] = [
  {
    label: "Default order",
    value: "",
  },
  {
    label: "Name: A to Z",
    value: AccountDashboardSortOrder.Name,
  },
  {
    label: "Name: Z to A",
    value: AccountDashboardSortOrder.NameDescending,
  },
  {
    label: "Type: A to Z",
    value: AccountDashboardSortOrder.Type,
  },
  {
    label: "Type: Z to A",
    value: AccountDashboardSortOrder.TypeDescending,
  },
  {
    label: "Opening balance: low to high",
    value: AccountDashboardSortOrder.OpeningBalance,
  },
  {
    label: "Opening balance: high to low",
    value: AccountDashboardSortOrder.OpeningBalanceDescending,
  },
  {
    label: "Ending balance: low to high",
    value: AccountDashboardSortOrder.ClosingBalance,
  },
  {
    label: "Ending balance: high to low",
    value: AccountDashboardSortOrder.ClosingBalanceDescending,
  },
];

/**
 * Account type options available in the Accounts dashboard controls.
 */
const accountTypeOptions: {
  readonly label: string;
  readonly value: AccountType | "";
}[] = [
  { label: "All account types", value: "" },
  ...Object.values(AccountType).map((accountType) => ({
    label: formatAccountType(accountType),
    value: accountType,
  })),
];

/**
 * Renders the hero-band controls for searching, sorting, and resetting the Accounts view.
 */
const AccountsDashboardControls = function ({
  accountingPeriods,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: AccountsDashboardControlsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const searchParamName = "search";
  const sortParamName = "sort";
  const pageParamName = "page";
  const modeParamName = "mode";
  const accountTypeParamName = "accountType";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const currentMode: AccountsDashboardFilterMode =
    searchParams.get(modeParamName) === "date" ? "date" : "accounting-period";
  const currentSearch = searchParams.get(searchParamName) ?? "";
  const currentSort = searchParams.get(sortParamName) ?? "";
  const currentAccountType = searchParams.get(accountTypeParamName) ?? "";
  const currentStartAccountingPeriodId =
    searchParams.get(startAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentEndAccountingPeriodId =
    searchParams.get(endAccountingPeriodIdParamName) ??
    defaultAccountingPeriodId ??
    "";
  const currentStartDate =
    searchParams.get(startDateParamName) ?? defaultStartDate;
  const currentEndDate = searchParams.get(endDateParamName) ?? defaultEndDate;

  const accountingPeriodIndexes = new Map(
    accountingPeriods.map((period, index) => [period.id, index]),
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    params.delete(pageParamName);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`);
  };

  const hasActiveView =
    currentMode !== "accounting-period" ||
    currentSearch.trim() !== "" ||
    currentSort !== "" ||
    currentAccountType !== "" ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentStartDate !== defaultStartDate ||
    currentEndDate !== defaultEndDate;

  const handleSearchChange = useDebouncedCallback(
    (event: React.ChangeEvent<HTMLInputElement>): void => {
      updateParams((params) => {
        const { value } = event.target;
        if (value.trim() !== "") {
          params.set(searchParamName, value);
        } else {
          params.delete(searchParamName);
        }
      });
    },
    300,
  );

  const handleSortChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    updateParams((params) => {
      const nextSort = event.target.value;
      if (nextSort === "") {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, nextSort);
      }
    });
  };

  const handleAccountTypeChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    updateParams((params) => {
      const nextAccountType = event.target.value;
      if (nextAccountType === "") {
        params.delete(accountTypeParamName);
      } else {
        params.set(accountTypeParamName, nextAccountType);
      }
    });
  };

  const handleModeChange = function (
    _: React.MouseEvent<HTMLElement>,
    nextMode: AccountsDashboardFilterMode | null,
  ): void {
    if (nextMode === null) {
      return;
    }

    updateParams((params) => {
      params.set(modeParamName, nextMode);
      if (nextMode === "date") {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
        params.set(
          startDateParamName,
          params.get(startDateParamName) ?? defaultStartDate,
        );
        params.set(
          endDateParamName,
          params.get(endDateParamName) ?? defaultEndDate,
        );
        return;
      }

      params.delete(startDateParamName);
      params.delete(endDateParamName);
      if (defaultAccountingPeriodId !== null) {
        params.set(
          startAccountingPeriodIdParamName,
          params.get(startAccountingPeriodIdParamName) ??
            defaultAccountingPeriodId,
        );
        params.set(
          endAccountingPeriodIdParamName,
          params.get(endAccountingPeriodIdParamName) ??
            defaultAccountingPeriodId,
        );
      }
    });
  };

  const handleStartAccountingPeriodChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const nextStartAccountingPeriodId = event.target.value;
    const nextStartIndex =
      accountingPeriodIndexes.get(nextStartAccountingPeriodId) ?? 0;
    const currentEndIndex =
      accountingPeriodIndexes.get(currentEndAccountingPeriodId) ??
      nextStartIndex;
    const nextEndAccountingPeriodId =
      nextStartIndex > currentEndIndex
        ? nextStartAccountingPeriodId
        : currentEndAccountingPeriodId;

    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
    });
  };

  const handleEndAccountingPeriodChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const nextEndAccountingPeriodId = event.target.value;
    const nextEndIndex =
      accountingPeriodIndexes.get(nextEndAccountingPeriodId) ?? 0;
    const currentStartIndex =
      accountingPeriodIndexes.get(currentStartAccountingPeriodId) ??
      nextEndIndex;
    const nextStartAccountingPeriodId =
      nextEndIndex < currentStartIndex
        ? nextEndAccountingPeriodId
        : currentStartAccountingPeriodId;

    updateParams((params) => {
      params.set(startAccountingPeriodIdParamName, nextStartAccountingPeriodId);
      params.set(endAccountingPeriodIdParamName, nextEndAccountingPeriodId);
    });
  };

  const handleStartDateChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const nextStartDate = event.target.value;
    const nextEndDate =
      nextStartDate > currentEndDate ? nextStartDate : currentEndDate;

    updateParams((params) => {
      params.set(startDateParamName, nextStartDate);
      params.set(endDateParamName, nextEndDate);
    });
  };

  const handleEndDateChange = function (
    event: React.ChangeEvent<HTMLInputElement>,
  ): void {
    const nextEndDate = event.target.value;
    const nextStartDate =
      nextEndDate < currentStartDate ? nextEndDate : currentStartDate;

    updateParams((params) => {
      params.set(startDateParamName, nextStartDate);
      params.set(endDateParamName, nextEndDate);
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(searchParamName);
      params.delete(sortParamName);
      params.delete(accountTypeParamName);
      params.set(modeParamName, "accounting-period");
      params.delete(startDateParamName);
      params.delete(endDateParamName);
      if (defaultAccountingPeriodId !== null) {
        params.set(startAccountingPeriodIdParamName, defaultAccountingPeriodId);
        params.set(endAccountingPeriodIdParamName, defaultAccountingPeriodId);
      } else {
        params.delete(startAccountingPeriodIdParamName);
        params.delete(endAccountingPeriodIdParamName);
      }
    });
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
          Dashboard filters
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Switch between accounting-period and date mode, then narrow the
          account set from one dashboard query.
        </Typography>
      </Stack>
      <ToggleButtonGroup
        exclusive
        value={currentMode}
        size="small"
        disabled={disabled}
        onChange={handleModeChange}
        sx={{ alignSelf: "flex-start" }}
      >
        <ToggleButton value="accounting-period">
          Accounting periods
        </ToggleButton>
        <ToggleButton value="date">Dates</ToggleButton>
      </ToggleButtonGroup>
      <Stack direction={{ xs: "column", lg: "row" }} spacing={1.5} useFlexGap>
        <TextField
          fullWidth
          size="small"
          placeholder="Search accounts"
          defaultValue={currentSearch}
          onChange={handleSearchChange}
          disabled={disabled}
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
          disabled={disabled}
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
          label="Account type"
          value={currentAccountType}
          onChange={handleAccountTypeChange}
          disabled={disabled}
          sx={{ minWidth: { xs: "100%", sm: 220 } }}
        >
          {accountTypeOptions.map((option) => (
            <MenuItem key={option.label} value={option.value}>
              {option.label}
            </MenuItem>
          ))}
        </TextField>
      </Stack>
      <Stack direction={{ xs: "column", lg: "row" }} spacing={1.5} useFlexGap>
        {currentMode === "accounting-period" ? (
          <>
            <TextField
              select
              size="small"
              label="Start period"
              value={currentStartAccountingPeriodId}
              onChange={handleStartAccountingPeriodChange}
              disabled={disabled || accountingPeriods.length === 0}
              sx={{ minWidth: { xs: "100%", sm: 220 } }}
            >
              {accountingPeriods.map((accountingPeriod) => (
                <MenuItem key={accountingPeriod.id} value={accountingPeriod.id}>
                  {accountingPeriod.name}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              size="small"
              label="End period"
              value={currentEndAccountingPeriodId}
              onChange={handleEndAccountingPeriodChange}
              disabled={disabled || accountingPeriods.length === 0}
              sx={{ minWidth: { xs: "100%", sm: 220 } }}
            >
              {accountingPeriods.map((accountingPeriod) => (
                <MenuItem key={accountingPeriod.id} value={accountingPeriod.id}>
                  {accountingPeriod.name}
                </MenuItem>
              ))}
            </TextField>
          </>
        ) : (
          <>
            <TextField
              size="small"
              label="Start date"
              type="date"
              value={currentStartDate}
              onChange={handleStartDateChange}
              disabled={disabled}
              sx={{ minWidth: { xs: "100%", sm: 220 } }}
              slotProps={{
                inputLabel: {
                  shrink: true,
                },
              }}
            />
            <TextField
              size="small"
              label="End date"
              type="date"
              value={currentEndDate}
              onChange={handleEndDateChange}
              disabled={disabled}
              sx={{ minWidth: { xs: "100%", sm: 220 } }}
              slotProps={{
                inputLabel: {
                  shrink: true,
                },
              }}
            />
          </>
        )}
        <Button
          variant="outlined"
          onClick={clearView}
          disabled={!hasActiveView}
          sx={{ flexShrink: 0 }}
        >
          Reset to current period
        </Button>
      </Stack>
    </Stack>
  );
};

export default AccountsDashboardControls;
