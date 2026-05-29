"use client";

import {
  Button,
  MenuItem,
  Paper,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from "@mui/material";
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/overview-dashboard/accountNameFilter";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/overview-dashboard/accountTypeFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountOverviewAccountNameFilter from "@/accounts/overview-dashboard/AccountOverviewAccountNameFilter";
import AccountOverviewAccountTypeFilter from "@/accounts/overview-dashboard/AccountOverviewAccountTypeFilter";
import type { AccountType } from "@/accounts/types";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { JSX } from "react";

/**
 * Dashboard filter mode values used in the Accounts view URL.
 */
type AccountsDashboardFilterMode = "accounting-period" | "date";

/**
 * Props for the AccountOverviewDashboardFilter component.
 */
interface AccountOverviewDashboardFilterProps {
  readonly accountingPeriods: readonly AccountingPeriod[];
  readonly availableAccountNames: readonly string[];
  readonly defaultAccountingPeriodId: string | null;
  readonly defaultStartDate: string;
  readonly defaultEndDate: string;
  readonly disabled?: boolean;
}

/**
 * Renders the dashboard filter card for the Accounts view.
 */
const AccountOverviewDashboardFilter = function ({
  accountingPeriods,
  availableAccountNames,
  defaultAccountingPeriodId,
  defaultStartDate,
  defaultEndDate,
  disabled = false,
}: AccountOverviewDashboardFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const pageParamName = "page";
  const modeParamName = "mode";
  const accountTypeParamName = "accountType";
  const accountNameParamName = "accountName";
  const startAccountingPeriodIdParamName = "startAccountingPeriodId";
  const endAccountingPeriodIdParamName = "endAccountingPeriodId";
  const startDateParamName = "startDate";
  const endDateParamName = "endDate";

  const currentMode: AccountsDashboardFilterMode =
    searchParams.get(modeParamName) === "date" ? "date" : "accounting-period";
  const currentAccountTypes = normalizeAccountTypes(
    searchParams.getAll(accountTypeParamName),
  );
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountNameParamName),
    availableAccountNames,
  );
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
    shouldPersistAccountTypes(currentAccountTypes) ||
    shouldPersistAccountNames(currentAccountNames) ||
    currentStartAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentEndAccountingPeriodId !== (defaultAccountingPeriodId ?? "") ||
    currentStartDate !== defaultStartDate ||
    currentEndDate !== defaultEndDate;

  const handleAccountTypeChange = function (
    nextAccountTypes: readonly AccountType[],
  ): void {
    updateParams((params) => {
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
      if (shouldPersistAccountTypes(nextAccountTypes)) {
        nextAccountTypes.forEach((accountType) => {
          params.append(accountTypeParamName, accountType);
        });
      }
    });
  };

  const handleAccountNameChange = function (
    nextAccountNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(accountNameParamName);
      if (shouldPersistAccountNames(nextAccountNames)) {
        nextAccountNames.forEach((accountName) => {
          params.append(accountNameParamName, accountName);
        });
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
      params.delete(accountTypeParamName);
      params.delete(accountNameParamName);
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
            <ToggleButton value="date">Dates</ToggleButton>
            <ToggleButton
              value="accounting-period"
              disabled={defaultAccountingPeriodId === null}
            >
              Accounting periods
            </ToggleButton>
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
          <AccountOverviewAccountTypeFilter
            value={currentAccountTypes}
            onChange={handleAccountTypeChange}
            disabled={disabled}
          />
          <AccountOverviewAccountNameFilter
            availableAccountNames={availableAccountNames}
            value={currentAccountNames}
            onChange={handleAccountNameChange}
            disabled={disabled}
          />
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
