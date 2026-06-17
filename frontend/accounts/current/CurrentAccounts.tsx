import type {
  AccountType,
  CurrentAccounts as CurrentAccountsModel,
} from "@/accounts/types";
import {
  normalizeAccountTypes,
  shouldPersistAccountTypes,
} from "@/accounts/trends/accountTypeFilter";
import {
  normalizeRequestedAccountNames,
  shouldPersistAccountNames,
} from "@/accounts/trends/accountNameFilter";
import CurrentAccountsFilter from "@/accounts/current/CurrentAccountsFilter";
import CurrentAccountsList from "@/accounts/current/CurrentAccountsList";
import CurrentAccountsSummaryCard from "@/accounts/current/CurrentAccountsSummaryCard";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import getApiClient from "@/framework/data/getApiClient";

interface CurrentAccountsSearchParams {
  accountType?: AccountType | readonly AccountType[];
  accountName?: string | readonly string[];
}

interface CurrentAccountsProps {
  readonly searchParams: Promise<CurrentAccountsSearchParams>;
}

const createEmptyCurrent = function (): CurrentAccountsModel {
  return {
    availableAccountNames: [],
    summary: {
      totalBalance: 0,
      totalTrackedBalance: 0,
      totalUntrackedBalance: 0,
      balanceByAccountType: [],
    },
    accounts: [],
  };
};

/**
 * Component that displays the current Accounts snapshot.
 */
const CurrentAccounts = async function ({
  searchParams,
}: CurrentAccountsProps): Promise<JSX.Element> {
  const { accountType, accountName } = await searchParams;

  const currentAccountTypes = normalizeAccountTypes(
    Array.isArray(accountType)
      ? accountType
      : typeof accountType === "string"
        ? [accountType]
        : [],
  );
  const currentAccountNames = normalizeRequestedAccountNames(
    Array.isArray(accountName)
      ? accountName
      : typeof accountName === "string"
        ? [accountName]
        : [],
  );

  const apiClient = getApiClient();
  const current: CurrentAccountsModel =
    (
      await apiClient.GET("/accounts/current", {
        params: {
          query: {
            ...(shouldPersistAccountTypes(currentAccountTypes)
              ? { AccountType: [...currentAccountTypes] }
              : {}),
            ...(shouldPersistAccountNames(currentAccountNames)
              ? { AccountName: [...currentAccountNames] }
              : {}),
          },
        },
      })
    ).data ?? createEmptyCurrent();

  return (
    <Stack spacing={3} sx={{ width: "100%" }}>
      <Stack spacing={3} sx={{ maxWidth: 1440, width: "100%" }}>
        <CurrentAccountsFilter
          availableAccountNames={current.availableAccountNames}
        />
      </Stack>
      <CurrentAccountsSummaryCard current={current} />
      <CurrentAccountsList current={current} />
    </Stack>
  );
};

export type { CurrentAccountsSearchParams };
export default CurrentAccounts;
