"use client";

import { Box, Button } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { BalanceEventType } from "@/balance-events/types";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { GoalBalanceEvent } from "@/goals/types";
import type { GoalWorkspaceSearchParams } from "@/goals/workspace/GoalWorkspace";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatLongDate } from "@/framework/dateHelpers";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";

/**
 * Props for the GoalBalanceEventsFrame component.
 */
interface GoalBalanceEventsFrameProps {
  readonly data: GoalBalanceEvent[] | null;
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

  const columns: ColumnDefinition<GoalBalanceEvent>[] = [
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? formatLongDate(new Date(`${balanceEvent.date}T00:00:00`))
          : "Pending",
      minWidth: 135,
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (balanceEvent) => (
        <Box
          component="span"
          sx={{
            color:
              balanceEvent.type === BalanceEventType.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {balanceEvent.type === BalanceEventType.Credit
            ? "Assignment"
            : "Spending"}
        </Box>
      ),
      minWidth: 130,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (balanceEvent) => formatCurrency(balanceEvent.amount),
      alignment: "right",
      minWidth: 120,
    },
    {
      name: "open",
      headerContent: "",
      getBodyContent: () => (
        <Box
          sx={{
            alignItems: "center",
            color: "text.secondary",
            display: "flex",
            justifyContent: "center",
            minHeight: 40,
          }}
        >
          <KeyboardArrowRight fontSize="small" />
        </Box>
      ),
      alignment: "center",
      minWidth: 0,
      maxWidth: 0,
    },
  ];

  return (
    <ConstrainedContent maxWidth={1200}>
      <ListFrame<GoalBalanceEvent>
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
