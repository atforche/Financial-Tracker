"use client";

import {
  Autocomplete,
  Button,
  Checkbox,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountingPeriod } from "@/accounting-periods/types";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";

/**
 * Props for the GoalWorkspaceFilter component.
 */
interface GoalWorkspaceFilterProps {
  readonly accountingPeriods: AccountingPeriod[];
  readonly funds: Fund[];
  readonly selectedAccountingPeriodId: string | null;
}

/** Filters the unified goal workspace by period and fund. */
const GoalWorkspaceFilter = function ({
  accountingPeriods,
  funds,
  selectedAccountingPeriodId,
}: GoalWorkspaceFilterProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const selectedPeriod =
    accountingPeriods.find(
      (period) => period.id === selectedAccountingPeriodId,
    ) ?? null;
  const selectedFundIds = new Set(searchParams.getAll("fundIds"));
  const selectedFunds = funds.filter((fund) => selectedFundIds.has(fund.id));

  const replace = function (update: (params: URLSearchParams) => void): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    const query = params.toString();
    router.replace(query === "" ? pathname : `${pathname}?${query}`, {
      scroll: false,
    });
  };

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
        maxWidth: 1440,
      }}
    >
      <Stack spacing={2}>
        <Typography variant="h5">Goals Workspace</Typography>
        <Stack direction="row" spacing={1.5} useFlexGap flexWrap="wrap">
          <Autocomplete
            size="small"
            options={accountingPeriods}
            value={selectedPeriod}
            sx={{ minWidth: { xs: "100%", sm: 260 }, flex: 1 }}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            getOptionLabel={(period) => period.name}
            onChange={(_, period) => {
              replace((params) => {
                if (period === null) {
                  params.delete("accountingPeriodId");
                } else {
                  params.set("accountingPeriodId", period.id);
                }
              });
            }}
            renderInput={(params) => (
              <TextField {...params} label="Accounting period" />
            )}
          />
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={funds}
            value={selectedFunds}
            sx={{ minWidth: { xs: "100%", sm: 280 }, flex: 1 }}
            limitTags={1}
            isOptionEqualToValue={(option, value) => option.id === value.id}
            getOptionLabel={(fund) => fund.name}
            onChange={(_, values) => {
              replace((params) => {
                params.delete("fundIds");
                values.forEach((fund) => {
                  params.append("fundIds", fund.id);
                });
              });
            }}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {option.name}
              </li>
            )}
            renderInput={(params) => (
              <TextField {...params} label="Funds" placeholder="All funds" />
            )}
          />
          <Button
            variant="outlined"
            onClick={() => {
              replace((params) => {
                params.delete("accountingPeriodId");
                params.delete("fundIds");
              });
            }}
            disabled={
              !searchParams.has("accountingPeriodId") &&
              selectedFunds.length === 0
            }
          >
            Reset filters
          </Button>
        </Stack>
      </Stack>
    </Paper>
  );
};

export default GoalWorkspaceFilter;
