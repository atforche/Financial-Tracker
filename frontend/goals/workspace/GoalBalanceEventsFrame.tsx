"use client";

import { Box, Button } from "@mui/material";
import {
  type GoalWorkspaceBalanceEvent,
  GoalWorkspaceBalanceEventType,
} from "@/goals/types";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import Frame from "@/framework/view/Frame";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";

/**
 * Props for the GoalBalanceEventsFrame component.
 */
interface GoalBalanceEventsFrameProps {
  readonly data: GoalWorkspaceBalanceEvent[] | null;
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
  const currentQuery = searchParams.toString();
  const returnUrl =
    currentQuery === "" ? pathname : `${pathname}?${currentQuery}`;

  const columns: ColumnDefinition<GoalWorkspaceBalanceEvent>[] = [
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        balanceEvent.isPosted
          ? dayjs(balanceEvent.date).format("MMMM D, YYYY")
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
              balanceEvent.type === GoalWorkspaceBalanceEventType.Spending
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {balanceEvent.type === GoalWorkspaceBalanceEventType.Assignment
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
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame
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
      >
        <ListFrame<GoalWorkspaceBalanceEvent>
          columns={columns}
          getId={(balanceEvent) =>
            `${balanceEvent.transactionId}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
          }
          data={data}
          totalCount={totalCount}
          searchParamName="balanceEventSearch"
          pageParamName="balanceEventPage"
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
      </Frame>
    </Box>
  );
};

export default GoalBalanceEventsFrame;
