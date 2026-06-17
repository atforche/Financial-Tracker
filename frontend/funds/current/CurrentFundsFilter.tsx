"use client";

import { Button, Paper, Stack, Typography } from "@mui/material";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { JSX } from "react";

interface CurrentFundsFilterProps {
  readonly availableFundNames: readonly string[];
  readonly disabled?: boolean;
}

/**
 * Filters the current funds page.
 */
const CurrentFundsFilter = function ({
  availableFundNames,
  disabled = false,
}: CurrentFundsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const fundNameParamName = "fundName";
  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundNameParamName),
    availableFundNames,
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    const nextQuery = params.toString();
    router.replace(nextQuery === "" ? pathname : `${pathname}?${nextQuery}`, {
      scroll: false,
    });
  };

  const handleFundNameChange = function (
    nextFundNames: readonly string[],
  ): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
      if (shouldPersistFundNames(nextFundNames)) {
        nextFundNames.forEach((fundName) => {
          params.append(fundNameParamName, fundName);
        });
      }
    });
  };

  const clearView = function (): void {
    updateParams((params) => {
      params.delete(fundNameParamName);
    });
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
        <Typography variant="h5">Current Funds</Typography>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <FundTrendsFundNameFilter
            availableFundNames={availableFundNames}
            value={currentFundNames}
            onChange={handleFundNameChange}
            disabled={disabled}
          />
          <Button
            variant="outlined"
            onClick={clearView}
            disabled={!shouldPersistFundNames(currentFundNames)}
            sx={{ flexShrink: 0 }}
          >
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default CurrentFundsFilter;
