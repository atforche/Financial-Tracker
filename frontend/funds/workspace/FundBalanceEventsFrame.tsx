"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { FundBalanceEvent } from "@/funds/types";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import createBalanceEventColumns from "@/balance-events/createBalanceEventColumns";
import { formatBalanceEventType } from "@/balance-events/helpers";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";

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

  const columns: readonly ColumnDefinition<FundBalanceEvent>[] =
    createBalanceEventColumns<FundBalanceEvent>({
      getTypeLabel: (event) =>
        formatBalanceEventType(event.type, event.isPosted),
      getPreviousBalance: (event) => event.previousBalance.postedBalance,
      getNewBalance: (event) => event.newBalance.postedBalance,
    });

  return (
    <ConstrainedContent maxWidth={1200}>
      <ListFrame<FundBalanceEvent>
        title="Recent Balance Events"
        color="info"
        headerContent={
          <Button
            component={Link}
            href={addTransactionHref}
            variant="contained"
          >
            Add Transaction
          </Button>
        }
        columns={columns}
        getId={(balanceEvent) =>
          `${balanceEvent.transactionId}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
        }
        data={data}
        totalCount={totalCount}
        pageParamName={propertyName<FundWorkspaceSearchParams>(
          "balanceEventPage",
        )}
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
    </ConstrainedContent>
  );
};

export default FundBalanceEventsFrame;
