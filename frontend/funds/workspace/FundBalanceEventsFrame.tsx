"use client";

import { Box, Button } from "@mui/material";
import type { FundWorkspaceBalanceEvent } from "@/funds/types";
import { BalanceEventTypeModel } from "@/framework/data/api";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import dayjs from "dayjs";
import formatCurrency from "@/framework/formatCurrency";
import routes from "@/transactions/routes";

/**
 * Props for the FundBalanceEventsFrame component.
 */
interface FundBalanceEventsFrameProps {
  readonly data: FundWorkspaceBalanceEvent[] | null;
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
  const currentQuery = searchParams.toString();
  const returnUrl =
    currentQuery === "" ? pathname : `${pathname}?${currentQuery}`;

  const columns: ColumnDefinition<FundWorkspaceBalanceEvent>[] = [
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
              balanceEvent.type === BalanceEventTypeModel.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {balanceEvent.type === BalanceEventTypeModel.Debit
            ? "Debit"
            : "Credit"}
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
      name: "before",
      headerContent: "Balance Before",
      getBodyContent: (balanceEvent) =>
        formatCurrency(balanceEvent.previousBalance.postedBalance),
      alignment: "right",
      minWidth: 150,
    },
    {
      name: "after",
      headerContent: "Balance After",
      getBodyContent: (balanceEvent) =>
        formatCurrency(balanceEvent.newBalance.postedBalance),
      alignment: "right",
      minWidth: 150,
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
      <ListFrame<FundWorkspaceBalanceEvent>
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
        searchParamName="balanceEventSearch"
        pageParamName="balanceEventPage"
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
    </Box>
  );
};

export default FundBalanceEventsFrame;
