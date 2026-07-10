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
import {
  normalizeAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import {
  normalizeFundNames,
  shouldPersistFundNames,
} from "@/funds/trends/fundNameFilter";
import {
  normalizeTransactionTypes,
  shouldPersistTransactionTypes,
  transactionTypeValues,
} from "@/transactions/trends/transactionTypeFilter";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AccountTrendsAccountNameFilter from "@/accounts/trends/AccountTrendsAccountNameFilter";
import FundTrendsFundNameFilter from "@/funds/trends/FundTrendsFundNameFilter";
import type { JSX } from "react";
import type { TransactionType } from "@/transactions/transaction";
import { buildUrl } from "@/framework/routes/helpers";

/**
 * Props for the CurrentTransactionsFilter component.
 */
interface CurrentTransactionsFilterProps {
  readonly availableAccountNames: readonly string[];
  readonly availableFundNames: readonly string[];
  readonly disabled?: boolean;
}

/**
 * Filter for the current transactions page.
 */
const CurrentTransactionsFilter = function ({
  availableAccountNames,
  availableFundNames,
  disabled = false,
}: CurrentTransactionsFilterProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const unpostedTransactionPageParamName = "unpostedTransactionPage";
  const postedTransactionPageParamName = "postedTransactionPage";
  const transactionTypeParamName = "transactionType";
  const accountNameParamName = "accountName";
  const fundNameParamName = "fundName";

  const currentTransactionTypes = normalizeTransactionTypes(
    searchParams.getAll(transactionTypeParamName),
  );
  const currentAccountNames = normalizeAccountNames(
    searchParams.getAll(accountNameParamName),
    availableAccountNames,
  );
  const currentFundNames = normalizeFundNames(
    searchParams.getAll(fundNameParamName),
    availableFundNames,
  );

  const updateParams = function (
    updater: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    updater(params);
    params.delete(unpostedTransactionPageParamName);
    params.delete(postedTransactionPageParamName);
    router.replace(buildUrl(pathname, params), {
      scroll: false,
    });
  };

  const handleTransactionTypeChange = function (
    nextTransactionTypes: readonly TransactionType[],
  ): void {
    updateParams((params) => {
      params.delete(transactionTypeParamName);
      if (shouldPersistTransactionTypes(nextTransactionTypes)) {
        nextTransactionTypes.forEach((transactionType) => {
          params.append(transactionTypeParamName, transactionType);
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
      params.delete(transactionTypeParamName);
      params.delete(accountNameParamName);
      params.delete(fundNameParamName);
    });
  };

  const hasActiveView =
    shouldPersistTransactionTypes(currentTransactionTypes) ||
    shouldPersistAccountNames(currentAccountNames) ||
    shouldPersistFundNames(currentFundNames);

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
          <Typography variant="h5">Current Transactions</Typography>
          <Typography color="text.secondary">
            Filter the live transaction snapshot by transaction type, account
            name, and fund name.
          </Typography>
        </Stack>
        <Stack
          direction="row"
          spacing={1.5}
          useFlexGap
          flexWrap="wrap"
          alignItems={{ xs: "stretch", md: "center" }}
        >
          <Autocomplete
            multiple
            disableCloseOnSelect
            size="small"
            options={[...transactionTypeValues]}
            value={[...currentTransactionTypes]}
            disabled={disabled}
            limitTags={1}
            sx={{ minWidth: { xs: "100%", sm: 280 }, flex: { md: 1 } }}
            noOptionsText="No transaction types found"
            slotProps={{
              paper: {
                sx: {
                  "& .MuiAutocomplete-listbox": {
                    maxHeight: 320,
                  },
                },
              },
            }}
            onChange={(_, nextTransactionTypes) => {
              handleTransactionTypeChange(nextTransactionTypes);
            }}
            renderOption={(props, option, { selected }) => (
              <li {...props}>
                <Checkbox size="small" checked={selected} sx={{ mr: 1 }} />
                {option}
              </li>
            )}
            renderInput={(params) => (
              <TextField
                {...params}
                label="Transaction types"
                {...(currentTransactionTypes.length === 0
                  ? { placeholder: "All transaction types" }
                  : {})}
              />
            )}
          />
          <AccountTrendsAccountNameFilter
            availableAccountNames={availableAccountNames}
            value={currentAccountNames}
            onChange={handleAccountNameChange}
            disabled={disabled}
          />
          <FundTrendsFundNameFilter
            availableFundNames={availableFundNames}
            value={currentFundNames}
            onChange={handleFundNameChange}
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

export default CurrentTransactionsFilter;
