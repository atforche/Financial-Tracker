"use client";

import {
  type Account,
  AccountSortOrder,
  formatAccountType,
} from "@/accounts/types";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/accounts/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountWorkspaceListFrame component.
 */
interface AccountWorkspaceListFrameProps {
  readonly data: Account[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays the paged account list for the workspace.
 */
const AccountWorkspaceListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: AccountWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const actionParamName = "action";
  const searchParamName = "search";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    const query = params.toString();
    router.replace(query === "" ? pathname : `${pathname}?${query}`, {
      scroll: false,
    });
  };

  const setSort = function (sort: AccountSortOrder | null): void {
    replaceSearchParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
      params.delete(pageParamName);
    });
  };

  const setAction = function (action: "create" | "onboard"): void {
    replaceSearchParams((params) => {
      params.set(actionParamName, action);
    });
  };

  const openAccount = function (accountId: string): void {
    const params = new URLSearchParams(searchParams.toString());
    const detailSearchParams: AccountWorkspaceSearchParams = {};
    const sort = tryParseEnum(
      AccountSortOrder,
      params.get(sortParamName) ?? "",
    );
    const search = params.get(searchParamName);
    const page = params.get(pageParamName);

    if (search !== null) {
      detailSearchParams.search = search;
    }
    if (sort !== null) {
      detailSearchParams.sort = sort;
    }
    if (page !== null) {
      detailSearchParams.page = page;
    }

    router.push(routes.workspaceDetail(accountId, detailSearchParams), {
      scroll: false,
    });
  };

  const currentSort = tryParseEnum(
    AccountSortOrder,
    searchParams.get(sortParamName) ?? "",
  );
  const addActionLabel = isInOnboardingMode
    ? "Onboard account"
    : "Create account";

  const columns: ColumnDefinition<Account>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account) => account.name,
      sortType:
        currentSort === AccountSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.NameDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account) => formatAccountType(account.type),
      sortType:
        currentSort === AccountSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "balance",
      headerContent: "Current Balance",
      getBodyContent: (account) =>
        formatCurrency(account.currentBalance.postedBalance),
      sortType:
        currentSort === AccountSortOrder.PostedBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.PostedBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.PostedBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.PostedBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "open",
      headerContent: "",
      getBodyContent: () => (
        <Box
          sx={{
            alignItems: "center",
            color: "text.secondary",
            display: "flex",
            justifyContent: "center",
            minHeight: 40,
          }}
        >
          <KeyboardArrowRight fontSize="small" />
        </Box>
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
  ];

  return (
    <Paper
      sx={{
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 3,
        p: { xs: 2, md: 2.5 },
      }}
    >
      <Stack spacing={2.5}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={1.5}
          justifyContent="space-between"
          alignItems={{ xs: "stretch", sm: "center" }}
        >
          <Typography variant="h6" color="text.secondary">
            Accounts
          </Typography>
          <Button
            variant="contained"
            onClick={() => {
              setAction(isInOnboardingMode ? "onboard" : "create");
            }}
          >
            {addActionLabel}
          </Button>
        </Stack>
        <ListFrame<Account>
          columns={columns}
          getId={(account) => account.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName={searchParamName}
          pageParamName={pageParamName}
          onRowClick={(account) => {
            openAccount(account.id);
          }}
          initialEmptyState={{
            title: "No accounts yet",
            description: isInOnboardingMode
              ? "Use the onboarding action to add the first account."
              : "Use the create action to add the first account.",
            action: null,
          }}
          filteredEmptyState={{
            title: "No accounts match this search",
            description:
              "Try a different name, type, or balance search to widen the list.",
            action: (
              <Button
                variant="contained"
                onClick={() => {
                  replaceSearchParams((params) => {
                    params.delete(searchParamName);
                    params.delete(pageParamName);
                    params.delete(actionParamName);
                  });
                }}
              >
                Clear search
              </Button>
            ),
          }}
        />
      </Stack>
    </Paper>
  );
};

export default AccountWorkspaceListFrame;
