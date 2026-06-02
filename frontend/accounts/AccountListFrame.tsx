"use client";

import {
  type Account,
  AccountSortOrder,
  formatAccountType,
} from "@/accounts/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
import { Button } from "@mui/material";
import ColumnButton from "@/framework/listframe/ColumnButton";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ColumnSortType from "@/framework/listframe/ColumnSortType";
import IconButton from "@/framework/listframe/IconButton";
import type { JSX } from "react";
import ListFrame from "@/framework/listframe/ListFrame";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/accounts/routes";
import tryParseEnum from "@/framework/data/tryParseEnum";

/**
 * Props for the AccountListFrame component.
 */
interface AccountListFrameProps {
  readonly data: Account[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
  readonly showCreateAction?: boolean;
}

/**
 * Component that displays the list of accounts associated with an accounting period.
 */
const AccountListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
  showCreateAction = true,
}: AccountListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const searchParamName = "search";
  const sortParamName = "sort";
  const pageParamName = "page";

  const setSort = function (sort: AccountSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<Account>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account: Account) => account.name,
      sortType:
        currentSort === AccountSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
      getBodyContent: (account: Account) => formatAccountType(account.type),
      sortType:
        currentSort === AccountSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
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
      name: "postedBalance",
      headerContent: "Posted Balance",
      getBodyContent: (account: Account) =>
        formatCurrency(account.currentBalance.postedBalance),
      sortType:
        currentSort === AccountSortOrder.PostedBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountSortOrder.PostedBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountSortOrder.PostedBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountSortOrder.PostedBalanceDescending);
        } else {
          setSort(null);
        }
      },
      minWidth: 125,
      alignment: "right",
    },
    {
      name: "actions",
      headerContent: showCreateAction ? (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(isInOnboardingMode ? routes.onboard : routes.create());
          }}
        />
      ) : (
        ""
      ),
      getBodyContent: (account: Account) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            router.push(routes.detail({ id: account.id }, {}));
          }}
        />
      ),
      alignment: "right",
      minWidth: 90,
    },
  ];

  return (
    <ListFrame<Account>
      columns={columns}
      getId={(account) => account.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      searchParamName={searchParamName}
      pageParamName={pageParamName}
      initialEmptyState={{
        title: "No accounts yet",
        description: isInOnboardingMode
          ? "Create your first account to finish onboarding and start tracking balances."
          : "Create an account to start tracking balances and transaction history.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              router.push(
                isInOnboardingMode ? routes.onboard : routes.create(),
              );
            }}
          >
            {isInOnboardingMode ? "Start onboarding" : "Create account"}
          </Button>
        ),
      }}
      filteredEmptyState={{
        title: "No accounts match this search",
        description:
          "Try a different account name or clear the current search to see all accounts.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              const params = new URLSearchParams(searchParams.toString());
              params.delete(searchParamName);
              params.delete(pageParamName);
              router.replace(`${pathname}?${params.toString()}`);
            }}
          >
            Clear search
          </Button>
        ),
      }}
    />
  );
};

export default AccountListFrame;
