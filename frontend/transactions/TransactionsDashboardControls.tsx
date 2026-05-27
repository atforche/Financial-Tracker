"use client";

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
import { TransactionSortOrder } from "@/transactions/types";
import { useDebouncedCallback } from "use-debounce";

/**
 * Interface representing an accounting period filter option.
 */
interface AccountingPeriodOption {
  readonly id: string;
  readonly name: string;
}

/**
 * Props for the TransactionsDashboardControls component.
 */
interface TransactionsDashboardControlsProps {
  readonly accountingPeriods: AccountingPeriodOption[];
  readonly accountingPeriodParamName: string;
  readonly searchParamName: string;
  readonly sortParamName: string;
  readonly pageParamName: string;
}

/**
 * Sort options available in the Transactions dashboard controls.
 */
const sortOptions: {
  readonly label: string;
  readonly value: TransactionSortOrder | "";
}[] = [
  {
    label: "Default order",
    value: "",
  },
  {
    label: "Date: oldest first",
    value: TransactionSortOrder.Date,
  },
  {
    label: "Date: newest first",
    value: TransactionSortOrder.DateDescending,
  },
  {
    label: "Location: A to Z",
    value: TransactionSortOrder.Location,
  },
  {
    label: "Location: Z to A",
    value: TransactionSortOrder.LocationDescending,
  },
  {
    label: "Debit from: A to Z",
    value: TransactionSortOrder.DebitFrom,
  },
  {
    label: "Debit from: Z to A",
    value: TransactionSortOrder.DebitFromDescending,
  },
  {
    label: "Credit to: A to Z",
    value: TransactionSortOrder.CreditTo,
  },
  {
    label: "Credit to: Z to A",
    value: TransactionSortOrder.CreditToDescending,
  },
  {
    label: "Amount: low to high",
    value: TransactionSortOrder.Amount,
  },
  {
    label: "Amount: high to low",
    value: TransactionSortOrder.AmountDescending,
  },
];

/**
 * Renders the hero-band controls for filtering, sorting, and resetting the Transactions view.
 */
const TransactionsDashboardControls = function ({
  accountingPeriods,
  accountingPeriodParamName,
  searchParamName,
  sortParamName,
  pageParamName,
}: TransactionsDashboardControlsProps): JSX.Element {
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
          Focus a period, change the ledger sort, and search across the current
          transaction view.
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
          <MenuItem value="">All periods</MenuItem>
          {accountingPeriods.map((accountingPeriod) => (
            <MenuItem key={accountingPeriod.id} value={accountingPeriod.id}>
              {accountingPeriod.name}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          fullWidth
          size="small"
          placeholder="Search transactions"
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

export default TransactionsDashboardControls;
