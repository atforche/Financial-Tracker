"use client";

import { Box, Button } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { BalanceEventType } from "@/balance-events/types";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import type { FundBalanceEvent } from "@/funds/types";
import type { FundWorkspaceSearchParams } from "@/funds/workspace/FundWorkspace";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import { formatBalanceEventType } from "@/balance-events/helpers";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatLongDate } from "@/framework/dateHelpers";
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

  const columns: ColumnDefinition<FundBalanceEvent>[] = [
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
          {formatBalanceEventType(balanceEvent.type, balanceEvent.isPosted)}
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
