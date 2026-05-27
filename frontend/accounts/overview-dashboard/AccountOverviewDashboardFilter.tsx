"use client";

import { AccountType, formatAccountType } from "@/accounts/types";
import {
  Button,
  InputAdornment,
  MenuItem,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";
import Search from "@mui/icons-material/Search";
import { useDebouncedCallback } from "use-debounce";

/**
 * Dashboard filter mode values used in the Accounts view URL.
 */
type AccountsDashboardFilterMode = "accounting-period" | "date";

/**
 * Props for the AccountOverviewDashboardFilter component.
 */
interface AccountOverviewDashboardFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Account type options available in the Accounts dashboard filters.
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
 * Renders the dashboard filter card for the Accounts view.
 */
const AccountOverviewDashboardFilter = function ({
  accountingPeriods,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: AccountOverviewDashboardFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const searchParamName = "search";
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

  const sharedFieldSx = {
    minWidth: { xs: "100%", sm: 180 },
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
        <Stack spacing={0.5}>
          <Typography variant="h5">Account Overview Dashboard</Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <ToggleButtonGroup
            exclusive
            value={currentMode}
            size="small"
            disabled={disabled}
            onChange={handleModeChange}
            sx={{ flexShrink: 0 }}
          >
            <ToggleButton value="accounting-period">
              Accounting periods
            </ToggleButton>
            <ToggleButton value="date">Dates</ToggleButton>
          </ToggleButtonGroup>
          {currentMode === "accounting-period" ? (
            <>
              <TextField
                select
                size="small"
                label="Start period"
                value={currentStartAccountingPeriodId}
                onChange={handleStartAccountingPeriodChange}
                disabled={disabled || accountingPeriods.length === 0}
                sx={sharedFieldSx}
              >
                {accountingPeriods.map((accountingPeriod) => (
                  <MenuItem
                    key={accountingPeriod.id}
                    value={accountingPeriod.id}
                  >
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
                sx={sharedFieldSx}
              >
                {accountingPeriods.map((accountingPeriod) => (
                  <MenuItem
                    key={accountingPeriod.id}
                    value={accountingPeriod.id}
                  >
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
                sx={sharedFieldSx}
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
                sx={sharedFieldSx}
                slotProps={{
                  inputLabel: {
                    shrink: true,
                  },
                }}
              />
            </>
          )}
          <TextField
            size="small"
            placeholder="Search accounts"
            defaultValue={currentSearch}
            onChange={handleSearchChange}
            disabled={disabled}
            sx={{ flex: 1, minWidth: { xs: "100%", md: 240 } }}
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
            label="Account type"
            value={currentAccountType}
            onChange={handleAccountTypeChange}
            disabled={disabled}
            sx={sharedFieldSx}
          >
            {accountTypeOptions.map((option) => (
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
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default AccountOverviewDashboardFilter;
