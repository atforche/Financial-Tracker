"use client";

import {
  type Account,
  AccountSortOrder,
  formatAccountType,
} from "@/accounts/types";
import { Button, Checkbox, Paper, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountWorkspaceListFrame component.
 */
interface AccountWorkspaceListFrameProps {
  readonly data: Account[] | null;
  readonly totalCount: number | null;
  readonly selectedAccountId: string | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Displays the paged account list for the workspace.
 */
const AccountWorkspaceListFrame = function ({
  data,
  totalCount,
  selectedAccountId,
  isInOnboardingMode,
}: AccountWorkspaceListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const sortParamName = "sort";
  const pageParamName = "page";
  const selectedAccountIdParamName = "selectedAccountId";
  const actionParamName = "action";
  const searchParamName = "search";

  const replaceSearchParams = function (
    update: (params: URLSearchParams) => void,
  ): void {
    const params = new URLSearchParams(searchParams.toString());
    update(params);
    router.replace(`${pathname}?${params.toString()}`, { scroll: false });
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

  const toggleSelection = function (accountId: string): void {
    replaceSearchParams((params) => {
      const currentlySelectedAccountId = params.get(selectedAccountIdParamName);
      if (currentlySelectedAccountId === accountId) {
        params.delete(selectedAccountIdParamName);
        return;
      }
      params.set(selectedAccountIdParamName, accountId);
    });
  };

  const currentSort = tryParseEnum(
    AccountSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<Account>[] = [
    {
      name: "selected",
      headerContent: "",
      getBodyContent: (account) => (
        <Checkbox
          checked={selectedAccountId === account.id}
          onClick={(event) => {
            event.stopPropagation();
            toggleSelection(account.id);
          }}
          slotProps={{
            input: {
              "aria-label": `Select ${account.name}`,
            },
          }}
        />
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
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
        <Typography variant="h6" color="text.secondary">
          Accounts
        </Typography>
        <ListFrame<Account>
          columns={columns}
          getId={(account) => account.id}
          data={data ?? null}
          totalCount={totalCount ?? null}
          searchParamName={searchParamName}
          pageParamName={pageParamName}
          onRowClick={(account) => {
            toggleSelection(account.id);
          }}
          isRowSelected={(account) => account.id === selectedAccountId}
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
                    params.delete(selectedAccountIdParamName);
                    if (
                      params.get(actionParamName) === "update" ||
                      params.get(actionParamName) === "delete"
                    ) {
                      params.delete(actionParamName);
                    }
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
