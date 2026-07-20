"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { BalanceEventType } from "@/balance-events/types";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { FundPlanBalanceEvent } from "@/goals/types";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import createBalanceEventColumns from "@/balance-events/createBalanceEventColumns";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";

/**
 * Props for the GoalBalanceEventsFrame component.
 */
interface GoalBalanceEventsFrameProps {
  readonly data: FundPlanBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly addTransactionHref: string;
  readonly accountingPeriodId: string;
  readonly fundId: string;
}

/**
 * Displays recent assignment and spending events within the goal workspace.
 */
const GoalBalanceEventsFrame = function ({
  data,
  totalCount,
  addTransactionHref,
  accountingPeriodId,
  fundId,
}: GoalBalanceEventsFrameProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const returnUrl = buildUrl(pathname, new URLSearchParams(searchParams));

  const columns: readonly ColumnDefinition<FundPlanBalanceEvent>[] =
    createBalanceEventColumns<FundPlanBalanceEvent>({
      getTypeLabel: (event) =>
        event.type === BalanceEventType.Credit ? "Assignment" : "Spending",
    });

  return (
    <ConstrainedContent maxWidth={1200}>
      <ListFrame<FundPlanBalanceEvent>
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
        pageParamName={propertyName<GoalWorkspaceSearchParams>(
          "balanceEventPage",
        )}
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
          title: "No balance events yet",
          description:
            "Create a transaction for this fund to start building its goal history.",
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

export default GoalBalanceEventsFrame;
