"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { BalanceEventType } from "@/balance-events/types";
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
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Balance event fields needed to list an account or fund balance event.
 */
interface WorkspaceBalanceEvent {
  readonly transactionId: string;
  readonly eventDate?: string | null;
  readonly type: BalanceEventType;
  readonly amount: number;
  readonly description: string;
  readonly isPosted: boolean;
  readonly previousBalance?: { readonly postedBalance: number } | null;
  readonly newBalance?: { readonly postedBalance: number } | null;
  readonly destinations: readonly { readonly displayName: string }[];
  readonly source:
    | { readonly displayName: string }
    | readonly { readonly displayName: string }[];
}

/**
 * Sort enum for a balance event list, which must support counterparty sorting.
 */
type BalanceEventSortEnum = Record<string, string> &
  Readonly<Record<"Counterparty" | "CounterpartyDescending", string>>;

/**
 * Props for the BalanceEventsFrame component.
 */
interface BalanceEventsFrameProps<T extends WorkspaceBalanceEvent> {
  readonly data: T[];
  readonly totalCount: number;
  readonly addTransactionHref: string;
  readonly sortEnum: BalanceEventSortEnum;
  readonly emptyDescription: string;
}

/**
 * Displays recent balance events within an account or fund workspace.
 */
const BalanceEventsFrame = function <T extends WorkspaceBalanceEvent>({
  data,
  totalCount,
  addTransactionHref,
  sortEnum,
  emptyDescription,
}: BalanceEventsFrameProps<T>): JSX.Element {
  const canWrite = useWriteAccess();
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const returnUrl = buildUrl(pathname, new URLSearchParams(searchParams));
  const pageParamName = "balanceEventPage";
  const sortParamName = "balanceEventSort";
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    sortEnum,
    searchParams.get(sortParamName) ?? "",
  );

  const setSort = function (sort: string | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const getSortProps = createColumnSortProps<string>(currentSort, setSort);

  const columns: readonly ColumnDefinition<T>[] = createBalanceEventColumns<T>({
    getPreviousBalance: (event) => event.previousBalance?.postedBalance ?? null,
    getNewBalance: (event) => event.newBalance?.postedBalance ?? null,
    getCounterpartyContent: formatBalanceEventCounterparty,
    counterpartySortProps: getSortProps(
      sortEnum.Counterparty,
      sortEnum.CounterpartyDescending,
    ),
  });

  const addTransactionButton = !canWrite ? null : (
    <Button component={Link} href={addTransactionHref} variant="contained">
      Add Transaction
    </Button>
  );

  return (
    <ListFrame<T>
      title="Recent Balance Events"
      desktopBreakpoint="xl"
      color="info"
      headerContent={addTransactionButton ?? undefined}
      columns={columns}
      getId={(balanceEvent) =>
        `${balanceEvent.transactionId}-${balanceEvent.eventDate}-${balanceEvent.type}-${balanceEvent.amount}`
      }
      data={data}
      totalCount={totalCount}
      pageParamName={pageParamName}
      onRowClick={(balanceEvent) => {
        router.push(
          routes.workspaceDetail(balanceEvent.transactionId, { returnUrl }),
        );
      }}
      hasActiveFilters={false}
      initialEmptyState={{
        title: "No Balance Events Yet",
        description: emptyDescription,
        action: addTransactionButton,
      }}
    />
  );
};

export type { WorkspaceBalanceEvent };
export default BalanceEventsFrame;
