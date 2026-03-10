'use client';

import { InputAdornment, TextField } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { JSX } from "react";
import { Search } from "@mui/icons-material";
import { useDebouncedCallback } from 'use-debounce';

/**
 * Props for the SearchBar component.
 */
interface SearchBarProps {
  readonly paramName: string;
}

/**
 * Component that renders a search bar that syncs its value with URL search parameters.
 */
const SearchBar = function ({
  paramName,
}: SearchBarProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const handleChange = useDebouncedCallback((event: React.ChangeEvent<HTMLInputElement>): void => {
    const params = new URLSearchParams(searchParams.toString());
    const { value } = event.target;
    if (value) {
      params.set(paramName, value);
    } else {
      params.delete(paramName);
    }
    router.replace(`${pathname}?${params.toString()}`);
  }, 300);

  return (
    <TextField
      size="small"
      placeholder="Search..."
      defaultValue={searchParams.get(paramName) ?? ""}
      onChange={handleChange}
      slotProps={{
        input: {
          startAdornment: (
            <InputAdornment position="start">
              <Search />
            </InputAdornment>
          ),
          sx: {
            maxWidth: "500px",
          }
        },
      }}
    />
  );
};

export default SearchBar;