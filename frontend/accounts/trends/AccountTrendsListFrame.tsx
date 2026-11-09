"use client";

import {
  type AccountWithBalanceRange,
  AccountWithBalanceRangeSort,
} from "@/accounts/types";
import PagedTable, { type PagedTableColumn } from "@/framework/view/PagedTable";
import {
  accountTrendsParamNames,
  clearAccountTrendsFilters,
  hasActiveAccountTrendsFilters,
} from "@/accounts/trends/helpers";
import { formatAccountType, isDebtAccountType } from "@/accounts/helpers";
import {
  formatCurrency,
  getCurrencyDifference,
} from "@/framework/currencyHelpers";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type { DetailViewParams } from "@/balance-events/getDetailViewParams";
import type { JSX } from "react";
import Link from "next/link";
import type { Route } from "next";
import SignedChange from "@/framework/view/SignedChange";
import routes from "@/accounts/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the AccountTrendsListFrame component.
 */
interface AccountTrendsListFrameProps {
  readonly data: readonly AccountWithBalanceRange[];
  readonly totalCount: number;
  readonly isInOnboardingMode: boolean;
  readonly transactionWorkspaceHref: Route;

  /**
   * The trends range and return link, so each account opens over the same range.
   */
  readonly detailParams: DetailViewParams;
}

const columns: readonly PagedTableColumn<AccountWithBalanceRange>[] = [
  {
    name: "name",
    header: "Name",
    getValue: (account) => account.name,
    width: "minmax(0, 2fr)",
    sort: [
      AccountWithBalanceRangeSort.Name,
      AccountWithBalanceRangeSort.NameDescending,
    ],
  },
  {
    name: "type",
    header: "Type",
    getValue: (account) => formatAccountType(account.type),
    width: "minmax(0, 1fr)",
    hideOnMobile: true,
    sort: [
      AccountWithBalanceRangeSort.Type,
      AccountWithBalanceRangeSort.TypeDescending,
    ],
  },
  {
    name: "startingBalance",
    header: "Starting Balance",
    getValue: (account) => formatCurrency(account.startingBalance),
    width: "minmax(0, 1fr)",
    alignment: "right",
    hideOnMobile: true,
    sort: [
      AccountWithBalanceRangeSort.StartingBalance,
      AccountWithBalanceRangeSort.StartingBalanceDescending,
    ],
  },
  {
    name: "endingBalance",
    header: "Ending Balance",
    getValue: (account) => formatCurrency(account.endingBalance),
    width: "minmax(0, 1fr)",
    alignment: "right",
    sort: [
      AccountWithBalanceRangeSort.EndingBalance,
      AccountWithBalanceRangeSort.EndingBalanceDescending,
    ],
  },
  {
    name: "change",
    header: "Net Change",
    getValue: (account) => (
      <SignedChange
        amount={getCurrencyDifference(
          account.endingBalance,
          account.startingBalance,
        )}
        isIncreaseGood={!isDebtAccountType(account.type)}
      />
    ),
    width: "minmax(0, 1fr)",
    alignment: "right",
    sort: [
      AccountWithBalanceRangeSort.NetChange,
      AccountWithBalanceRangeSort.NetChangeDescending,
    ],
  },
];

/**
 * Presents the paged accounts for the Accounts trends, each linking to the account over the same range.
 */
const AccountTrendsListFrame = function ({
  data,
  totalCount,
  isInOnboardingMode,
  transactionWorkspaceHref,
  detailParams,
}: AccountTrendsListFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const searchParams = useSearchParams();
  const router = useRouter();
  const updateParams = useSearchParamUpdater([accountTrendsParamNames.page]);

  const emptyState = hasActiveAccountTrendsFilters(searchParams)
    ? {
        title: "No Accounts Match This Trends Filter",
        description:
          "Try a different account type, account name, or date range to widen the trends scope.",
        action: (
          <Button
            variant="contained"
            onClick={() => {
              updateParams((params) => {
                clearAccountTrendsFilters(params);
              });
            }}
          >
            Reset filters
          </Button>
        ),
      }
    : {
        title: "No Accounts Have Been Added",
        description: isInOnboardingMode
          ? "Onboard a new account to start tracking balances."
          : "Create a new account to start tracking balances.",
        action: !canWrite ? null : (
          <Button
            variant="contained"
            onClick={() => {
              router.push(
                routes.workspace({
                  action: isInOnboardingMode ? "onboard" : "create",
                }),
              );
            }}
          >
            {isInOnboardingMode ? "Onboard account" : "Create account"}
          </Button>
        ),
      };

  return (
    <PagedTable
      title="Accounts"
      headerContent={
        <Button
          component={Link}
          href={transactionWorkspaceHref}
          variant="outlined"
        >
          View transactions
        </Button>
      }
      columns={columns}
      data={data}
      getKey={(account) => account.id}
      getHref={(account) => routes.workspaceDetail(account.id, detailParams)}
      totalCount={totalCount}
      sortParamName={accountTrendsParamNames.sort}
      pageParamName={accountTrendsParamNames.page}
      emptyState={emptyState}
    />
  );
};

export default AccountTrendsListFrame;
