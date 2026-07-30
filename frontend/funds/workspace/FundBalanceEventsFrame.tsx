"use client";

import { type FundBalanceEvent, FundBalanceEventSort } from "@/funds/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/types";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import createBalanceEventColumns from "@/balance-events/createBalanceEventColumns";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatBalanceEventCounterparty } from "@/balance-events/helpers";
import parseEnumValue from "@/framework/data/parseEnumValue";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";
import useSearchParamUpdater from "@/framework/routes/useSearchParamUpdater";

/**
 * Props for the FundBalanceEventsFrame component.
 */
interface FundBalanceEventsFrameProps {
  readonly data: FundBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly addTransactionHref: string;
}

/**
 * Displays recent fund balance events within the fund workspace.
 */
const FundBalanceEventsFrame = function ({
  data,
  totalCount,
  addTransactionHref,
}: FundBalanceEventsFrameProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const returnUrl = buildUrl(pathname, new URLSearchParams(searchParams));
  const pageParamName =
    propertyName<FundWorkspaceSearchParams>("balanceEventPage");
  const sortParamName =
    propertyName<FundWorkspaceSearchParams>("balanceEventSort");
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    FundBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );

  const setSort = function (sort: FundBalanceEventSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: readonly ColumnDefinition<FundBalanceEvent>[] =
    createBalanceEventColumns<FundBalanceEvent>({
      getPreviousBalance: (event) => event.previousBalance.postedBalance,
      getNewBalance: (event) => event.newBalance.postedBalance,
      getCounterpartyContent: formatBalanceEventCounterparty,
      counterpartySortProps: getSortProps(
        FundBalanceEventSort.Counterparty,
        FundBalanceEventSort.CounterpartyDescending,
      ),
    });

  return (
    <ListFrame<FundBalanceEvent>
      title="Recent Balance Events"
      color="info"
      headerContent={
        <Button component={Link} href={addTransactionHref} variant="contained">
          Add Transaction
        </Button>
      }
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
          { scroll: false },
        );
      }}
      hasActiveFilters={false}
      initialEmptyState={{
        title: "No balance events yet",
        description:
          "Create a transaction for this fund to start building its balance history.",
        action: (
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

export default FundBalanceEventsFrame;
