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
import { AccountSortOrder } from "@/accounts/types";
import type { JSX } from "react";
import { Search } from "@mui/icons-material";
import { useDebouncedCallback } from "use-debounce";

/**
 * Props for the AccountsDashboardControls component.
 */
interface AccountsDashboardControlsProps {
  readonly searchParamName: string;
  readonly sortParamName: string;
  readonly pageParamName: string;
}

/**
 * Sort options available in the Accounts dashboard controls.
 */
const sortOptions: {
  readonly label: string;
  readonly value: AccountSortOrder | "";
}[] = [
  {
    label: "Default order",
    value: "",
  },
  {
    label: "Name: A to Z",
    value: AccountSortOrder.Name,
  },
  {
    label: "Name: Z to A",
    value: AccountSortOrder.NameDescending,
  },
  {
    label: "Type: A to Z",
    value: AccountSortOrder.Type,
  },
  {
    label: "Type: Z to A",
    value: AccountSortOrder.TypeDescending,
  },
  {
    label: "Balance: low to high",
    value: AccountSortOrder.PostedBalance,
  },
  {
    label: "Balance: high to low",
    value: AccountSortOrder.PostedBalanceDescending,
  },
];

/**
 * Renders the hero-band controls for searching, sorting, and resetting the Accounts view.
 */
const AccountsDashboardControls = function ({
  searchParamName,
  sortParamName,
  pageParamName,
}: AccountsDashboardControlsProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const currentSearch = searchParams.get(searchParamName) ?? "";
  const currentSort = searchParams.get(sortParamName) ?? "";
  const hasActiveView = currentSearch.trim() !== "" || currentSort !== "";

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

  const clearView = function (): void {
    const params = new URLSearchParams(searchParams.toString());
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
          Refine the registry before you move into row-level detail.
        </Typography>
      </Stack>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} useFlexGap>
        <TextField
          fullWidth
          size="small"
          placeholder="Search accounts"
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

export default AccountsDashboardControls;
