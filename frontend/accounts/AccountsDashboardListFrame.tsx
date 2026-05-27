"use client";

import {
  type AccountDashboardAccount,
  AccountDashboardSortOrder,
  formatAccountType,
  isPositiveChangeInBalance,
} from "@/accounts/types";
import { Button, Stack, Typography } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import AddCircleOutline from "@mui/icons-material/AddCircleOutline";
import ArrowForwardIos from "@mui/icons-material/ArrowForwardIos";
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
 * Props for the AccountsDashboardListFrame component.
 */
interface AccountsDashboardListFrameProps {
  readonly data: AccountDashboardAccount[] | null;
  readonly totalCount: number | null;
  readonly isInOnboardingMode: boolean;
}

/**
 * Presents the paged account table for the Accounts dashboard.
 */
const AccountsDashboardListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
}: AccountsDashboardListFrameProps): JSX.Element {
  const searchParams = useSearchParams();
  const pathname = usePathname();
  const router = useRouter();

  const searchParamName = "search";
  const sortParamName = "sort";
  const pageParamName = "page";

  const setSort = function (sort: AccountDashboardSortOrder | null): void {
    const params = new URLSearchParams(searchParams.toString());
    if (sort === null) {
      params.delete(sortParamName);
    } else {
      params.set(sortParamName, sort);
    }
    params.delete(pageParamName);
    router.replace(`${pathname}?${params.toString()}`);
  };

  const currentSort = tryParseEnum(
    AccountDashboardSortOrder,
    searchParams.get(sortParamName) ?? "",
  );

  const columns: ColumnDefinition<AccountDashboardAccount>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account) => account.name,
      sortType:
        currentSort === AccountDashboardSortOrder.Name
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.NameDescending);
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
        currentSort === AccountDashboardSortOrder.Type
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.TypeDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.Type);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.TypeDescending);
        } else {
          setSort(null);
        }
      },
    },
    {
      name: "startingBalance",
      headerContent: "Starting Balance",
      getBodyContent: (account) => formatCurrency(account.startingBalance),
      sortType:
        currentSort === AccountDashboardSortOrder.OpeningBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.OpeningBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.OpeningBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.OpeningBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "endingBalance",
      headerContent: "Ending Balance",
      getBodyContent: (account) => formatCurrency(account.endingBalance),
      sortType:
        currentSort === AccountDashboardSortOrder.ClosingBalance
          ? ColumnSortType.Ascending
          : currentSort === AccountDashboardSortOrder.ClosingBalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSort(AccountDashboardSortOrder.ClosingBalance);
        } else if (sortType === ColumnSortType.Descending) {
          setSort(AccountDashboardSortOrder.ClosingBalanceDescending);
        } else {
          setSort(null);
        }
      },
      alignment: "right",
      minWidth: 140,
    },
    {
      name: "change",
      headerContent: "Range Change",
      getBodyContent: (account): JSX.Element => {
        const changeInBalance = account.endingBalance - account.startingBalance;
        const isPositive = isPositiveChangeInBalance(
          account.type,
          changeInBalance,
        );

        return (
          <Stack spacing={0.25} alignItems="flex-end">
            <Typography
              variant="body2"
              fontWeight={700}
              color={isPositive ? "success.main" : "error.main"}
            >
              {formatCurrency(changeInBalance)}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {formatCurrency(account.startingBalance)} to{" "}
              {formatCurrency(account.endingBalance)}
            </Typography>
          </Stack>
        );
      },
      alignment: "right",
      minWidth: 160,
    },
    {
      name: "actions",
      headerContent: (
        <IconButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            router.push(
              isInOnboardingMode ? routes.onboard : routes.create({}),
            );
          }}
        />
      ),
      getBodyContent: (account) => (
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
    <ListFrame<AccountDashboardAccount>
      columns={columns}
      getId={(account) => account.id}
      data={data ?? null}
      totalCount={totalCount ?? null}
      searchParamName={searchParamName}
      pageParamName={pageParamName}
      initialEmptyState={{
        title: "No accounts in this range",
        description: isInOnboardingMode
          ? "Start onboarding to create your first account and populate the dashboard."
          : "No accounts fall inside the selected dashboard range yet.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              router.push(
                isInOnboardingMode ? routes.onboard : routes.create({}),
              );
            }}
          >
            {isInOnboardingMode ? "Start onboarding" : "Create account"}
          </Button>
        ),
      }}
      filteredEmptyState={{
        title: "No accounts match this dashboard filter",
        description:
          "Try a different search, account type, or date range to widen the dashboard scope.",
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

export default AccountsDashboardListFrame;
