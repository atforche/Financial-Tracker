"use client";

import { Box, Button } from "@mui/material";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountBalanceEvent } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/AccountWorkspace";
import { BalanceEventType } from "@/framework/data/types";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import dayjs from "dayjs";
import { formatBalanceEventType } from "@/framework/data/helpers";
import { formatCurrency } from "@/framework/currencyHelpers";
import nameof from "@/framework/data/nameof";
import routes from "@/transactions/routes";

/**
 * Props for the AccountBalanceEventsFrame component.
 */
interface AccountBalanceEventsFrameProps {
  readonly data: AccountBalanceEvent[] | null;
  readonly totalCount: number | null;
  readonly addTransactionHref: string;
}

/**
 * Displays recent account balance events within the account workspace.
 */
const AccountBalanceEventsFrame = function ({
  data,
  totalCount,
  addTransactionHref,
}: AccountBalanceEventsFrameProps): JSX.Element {
  const pathname = usePathname();
  const router = useRouter();
  const searchParams = useSearchParams();
  const currentQuery = searchParams.toString();
  const returnUrl =
    currentQuery === "" ? pathname : `${pathname}?${currentQuery}`;

  const columns: ColumnDefinition<AccountBalanceEvent>[] = [
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (balanceEvent) =>
        dayjs(balanceEvent.date).format("MMMM D, YYYY"),
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
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <ListFrame<AccountBalanceEvent>
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
        pageParamName={nameof<AccountWorkspaceSearchParams>("balanceEventPage")}
        onRowClick={(balanceEvent) => {
          router.push(
            routes.workspaceDetail(balanceEvent.transactionId, {
              returnUrl,
            }),
            { scroll: false },
          );
        }}
        hasActiveFilters={false}
        initialEmptyState={{
          title: "No balance events yet",
          description:
            "Create or post a transaction for this account to start building its balance history.",
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

export default AccountBalanceEventsFrame;
