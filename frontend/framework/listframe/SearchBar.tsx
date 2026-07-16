"use client";

import { InputAdornment, TextField } from "@mui/material";
import type { JSX } from "react";
import { Search } from "@mui/icons-material";
import { isNotNullOrUndefined } from "@/framework/nullHelpers";
import { useDebouncedCallback } from "use-debounce";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useSearchParams } from "next/navigation";

/**
 * Props for the SearchBar component.
 */
interface SearchBarProps {
  readonly searchParamName: string;
  readonly pageParamName?: string;
}

/**
 * Component that renders a search bar that syncs its value with URL search parameters.
 */
const SearchBar = function ({
  searchParamName,
  pageParamName,
}: SearchBarProps): JSX.Element {
  const searchParams = useSearchParams();
  const updateParams = useSearchParamUpdater(
    isNotNullOrUndefined(pageParamName) ? [pageParamName] : [],
  );

  const handleChange = useDebouncedCallback(
    (event: React.ChangeEvent<HTMLInputElement>): void => {
      const { value } = event.target;
      updateParams((params) => {
        if (value) {
          params.set(searchParamName, value);
        } else {
          params.delete(searchParamName);
        }
      });
    },
    300,
  );

  return (
    <TextField
      size="small"
      placeholder="Search..."
      defaultValue={searchParams.get(searchParamName) ?? ""}
      onChange={handleChange}
      fullWidth
      sx={{
        flex: "1 1 280px",
        minWidth: 0,
      }}
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
  );
};

export default SearchBar;
