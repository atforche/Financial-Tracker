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
import { FundSortOrder } from "@/funds/types";
import type { JSX } from "react";
import { Search } from "@mui/icons-material";
import { useDebouncedCallback } from "use-debounce";

/**
 * Props for the FundsDashboardControls component.
 */
interface FundsDashboardControlsProps {
  readonly searchParamName: string;
  readonly sortParamName: string;
  readonly pageParamName: string;
}

/**
 * Sort options available in the Funds dashboard controls.
 */
const sortOptions: {
  readonly label: string;
  readonly value: FundSortOrder | "";
}[] = [
  {
    label: "Default order",
    value: "",
  },
  {
    label: "Name: A to Z",
    value: FundSortOrder.Name,
  },
  {
    label: "Name: Z to A",
    value: FundSortOrder.NameDescending,
  },
  {
    label: "Description: A to Z",
    value: FundSortOrder.Description,
  },
  {
    label: "Description: Z to A",
    value: FundSortOrder.DescriptionDescending,
  },
  {
    label: "Balance: low to high",
    value: FundSortOrder.Balance,
  },
  {
    label: "Balance: high to low",
    value: FundSortOrder.BalanceDescending,
  },
];

/**
 * Renders the hero-band controls for searching, sorting, and resetting the Funds view.
 */
const FundsDashboardControls = function ({
  searchParamName,
  sortParamName,
  pageParamName,
}: FundsDashboardControlsProps): JSX.Element {
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
          Refine the fund registry before you move into individual balances.
        </Typography>
      </Stack>
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} useFlexGap>
        <TextField
          fullWidth
          size="small"
          placeholder="Search funds"
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

export default FundsDashboardControls;
