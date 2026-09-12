"use client";

import {
  type AccountBalanceEvent,
  AccountBalanceEventSort,
} from "@/accounts/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountGoalWorkspaceSearchParams } from "@/account-goals/workspace/types";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import createBalanceEventColumns from "@/balance-events/createBalanceEventColumns";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatBalanceEventCounterparty } from "@/balance-events/helpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import transactionRoutes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

interface AccountGoalBalanceEventsFrameProps {
  readonly data: AccountBalanceEvent[];
  readonly totalCount: number;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly accountId: string;
}

/** Displays the account's balance events for the goal's accounting period. */
const AccountGoalBalanceEventsFrame = function ({
  data,
  totalCount,
  addTransactionHref,
  accountingPeriodId,
  accountId,
}: AccountGoalBalanceEventsFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const returnUrl = buildUrl(pathname, new URLSearchParams(searchParams));
  const pageParamName =
    propertyName<AccountGoalWorkspaceSearchParams>("balanceEventPage");
  const sortParamName =
    propertyName<AccountGoalWorkspaceSearchParams>("balanceEventSort");
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    AccountBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );
  const setSort = (sort: AccountBalanceEventSort | null): void => {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };
  const getSortProps = createColumnSortProps(currentSort, setSort);
  const columns: readonly ColumnDefinition<AccountBalanceEvent>[] =
    createBalanceEventColumns<AccountBalanceEvent>({
      dateSortProps: getSortProps(
        AccountBalanceEventSort.Date,
        AccountBalanceEventSort.DateDescending,
      ),
      typeSortProps: getSortProps(
        AccountBalanceEventSort.Type,
        AccountBalanceEventSort.TypeDescending,
      ),
      amountSortProps: getSortProps(
        AccountBalanceEventSort.Amount,
        AccountBalanceEventSort.AmountDescending,
      ),
      getCounterpartyContent: formatBalanceEventCounterparty,
      counterpartySortProps: getSortProps(
        AccountBalanceEventSort.Counterparty,
        AccountBalanceEventSort.CounterpartyDescending,
      ),
    });

  return (
    <ListFrame<AccountBalanceEvent>
      title="Recent Balance Events"
      desktopBreakpoint="xl"
      color="info"
      headerContent={
        !canWrite ? undefined : (
          <Button
            component={Link}
            href={addTransactionHref}
            variant="contained"
          >
            Add Transaction
          </Button>
        )
      }
      columns={columns}
      getId={(event) =>
        `${event.transactionId}-${event.eventDate}-${event.type}-${event.amount}`
      }
      data={data}
      totalCount={totalCount}
      pageParamName={pageParamName}
      onRowClick={(event) => {
        router.push(
          transactionRoutes.workspaceDetail(event.transactionId, {
            accountingPeriodIds: [accountingPeriodId],
            accountIds: [accountId],
            returnUrl,
          }),
          {
            scroll: false,
          },
        );
      }}
      hasActiveFilters={false}
      initialEmptyState={{
        title: "No Balance Events Yet",
        description:
          "Create or post a transaction for this account to build its goal history.",
        action: !canWrite ? null : (
          <Button
            component={Link}
            href={addTransactionHref}
            variant="contained"
          >
            Add Transaction
          </Button>
        ),
      }}
    />
  );
};

export default AccountGoalBalanceEventsFrame;
