"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import type { AccountBalanceEvent } from "@/accounts/types";
import type { AccountWorkspaceSearchParams } from "@/accounts/workspace/types";
import { Button } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type { JSX } from "react";
import Link from "next/link";
import ListFrame from "@/framework/listframe/ListFrame";
import { buildUrl } from "@/framework/routes/helpers";
import createBalanceEventColumns from "@/balance-events/createBalanceEventColumns";
import { formatBalanceEventType } from "@/balance-events/helpers";
import propertyName from "@/framework/data/propertyName";
import routes from "@/transactions/routes";

/**
 * Props for the AccountBalanceEventsFrame component.
 */
interface AccountBalanceEventsFrameProps {
  readonly data: AccountBalanceEvent[];
  readonly totalCount: number;
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
  const returnUrl = buildUrl(pathname, new URLSearchParams(searchParams));

  const columns: readonly ColumnDefinition<AccountBalanceEvent>[] =
    createBalanceEventColumns<AccountBalanceEvent>({
      getTypeLabel: (event) =>
        formatBalanceEventType(event.type, event.isPosted),
      getPreviousBalance: (event) => event.previousBalance.postedBalance,
      getNewBalance: (event) => event.newBalance.postedBalance,
    });

  return (
    <ListFrame<AccountBalanceEvent>
      title="Recent Balance Events"
      color="info"
      headerContent={
        <Button component={Link} href={addTransactionHref} variant="contained">
          Add Transaction
        </Button>
      }
      columns={columns}
      getId={(balanceEvent) =>
        `${balanceEvent.transactionId}-${balanceEvent.date}-${balanceEvent.type}-${balanceEvent.amount}`
      }
      data={data}
      totalCount={totalCount}
      pageParamName={propertyName<AccountWorkspaceSearchParams>(
        "balanceEventPage",
      )}
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
  );
};

export default AccountBalanceEventsFrame;
