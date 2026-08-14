"use client";

import {
  type FundGoalBalanceEvent,
  FundGoalBalanceEventSort,
} from "@/fund-goals/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { FundGoalWorkspaceSearchParams } from "@/fund-goals/workspace/FundGoalWorkspace";
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
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the FundGoalBalanceEventsFrame component.
 */
interface FundGoalBalanceEventsFrameProps {
  readonly data: FundGoalBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly fundId: string;
}

/**
 * Displays recent assignment and spending events for a Fund Goal.
 */
const FundGoalBalanceEventsFrame = function ({
  data,
  totalCount,
  addTransactionHref,
  accountingPeriodId,
  fundId,
}: FundGoalBalanceEventsFrameProps): JSX.Element {
  const canWrite = useWriteAccess();
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const returnUrl = buildUrl(pathname, new URLSearchParams(searchParams));
  const pageParamName =
    propertyName<FundGoalWorkspaceSearchParams>("balanceEventPage");
  const sortParamName =
    propertyName<FundGoalWorkspaceSearchParams>("balanceEventSort");
  const updateParams = useSearchParamUpdater([pageParamName]);
  const currentSort = parseEnumValue(
    FundGoalBalanceEventSort,
    searchParams.get(sortParamName) ?? "",
  );

  const setSort = function (sort: FundGoalBalanceEventSort | null): void {
    updateParams((params) => {
      if (sort === null) {
        params.delete(sortParamName);
      } else {
        params.set(sortParamName, sort);
      }
    });
  };

  const getSortProps = createColumnSortProps(currentSort, setSort);

  const columns: readonly ColumnDefinition<FundGoalBalanceEvent>[] =
    createBalanceEventColumns<FundGoalBalanceEvent>({
      getCounterpartyContent: formatBalanceEventCounterparty,
      counterpartySortProps: getSortProps(
        FundGoalBalanceEventSort.Counterparty,
        FundGoalBalanceEventSort.CounterpartyDescending,
      ),
    });

  return (
    <ConstrainedContent maxWidth={1200}>
      <ListFrame<FundGoalBalanceEvent>
        title="Recent Balance Events"
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
        getId={(balanceEvent) =>
          `${balanceEvent.transactionId}-${balanceEvent.eventDate}-${balanceEvent.type}-${balanceEvent.amount}`
        }
        data={data}
        totalCount={totalCount}
        pageParamName={pageParamName}
        onRowClick={(balanceEvent) => {
          router.push(
            routes.workspaceDetail(balanceEvent.transactionId, {
              accountingPeriodIds: [accountingPeriodId],
              fundIds: [fundId],
              returnUrl,
            }),
            { scroll: false },
          );
        }}
        hasActiveFilters={false}
        initialEmptyState={{
          title: "No Balance Events Yet",
          description:
            "Create a transaction for this fund to start building its Fund Goal history.",
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
    </ConstrainedContent>
  );
};

export default FundGoalBalanceEventsFrame;
