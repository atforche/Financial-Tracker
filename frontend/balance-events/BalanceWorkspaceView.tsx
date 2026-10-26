"use client";

import BalanceEventsFrame, {
  type WorkspaceBalanceEvent,
} from "@/balance-events/BalanceEventsFrame";
import { Box, Button, Stack, Tab, Tabs, Typography } from "@mui/material";
import {
  type JSX,
  type ReactNode,
  startTransition,
  useActionState,
  useState,
} from "react";
import {
  formatCurrency,
  formatSignedCurrency,
} from "@/framework/currencyHelpers";
import type { BalanceEventTotals } from "@/balance-events/types";
import type { BalanceTrendDateSummary } from "@/framework/charts/balanceTrendHelpers";
import ConfirmActionDialog from "@/framework/dialog/ConfirmActionDialog";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import Frame from "@/framework/view/Frame";
import PageLayout from "@/framework/view/PageLayout";
import RecentBalanceActivity from "@/balance-events/RecentBalanceActivity";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import type { Route } from "next";
import type { WorkspaceActionState } from "@/framework/forms/workspaceAction";
import { useWriteAccess } from "@/framework/auth/ApplicationUserProvider";

/**
 * Props for the BalanceWorkspaceView component.
 */
interface BalanceWorkspaceViewProps<
  TEvent extends WorkspaceBalanceEvent,
  TDeletePayload,
> {
  readonly entityName: "account" | "fund";
  readonly name: string;
  readonly summaryFields: ReactNode;
  readonly postedBalance: number;
  readonly pendingAmount: number;
  readonly deleteAction: (
    state: WorkspaceActionState,
    payload: TDeletePayload,
  ) => Promise<WorkspaceActionState>;
  readonly deletePayload: TDeletePayload;
  readonly renderUpdateForm: (onClose: () => void) => ReactNode;
  readonly recentBalanceEvents: TEvent[];
  readonly recentBalanceEventCount: number;
  readonly balanceEventSort: Parameters<
    typeof BalanceEventsFrame
  >[0]["sortEnum"];
  readonly emptyBalanceEventsDescription: string;
  readonly activityTotals: BalanceEventTotals;
  readonly recentActivityBalances: readonly BalanceTrendDateSummary[];
  readonly trendsHref: Route;
  readonly addTransactionHref: string;
}

/**
 * Displays the read-only workspace view for a selected account or fund.
 */
const BalanceWorkspaceView = function <
  TEvent extends WorkspaceBalanceEvent,
  TDeletePayload,
>({
  entityName,
  name,
  summaryFields,
  postedBalance,
  pendingAmount,
  deleteAction,
  deletePayload,
  renderUpdateForm,
  recentBalanceEvents,
  recentBalanceEventCount,
  balanceEventSort,
  emptyBalanceEventsDescription,
  activityTotals,
  recentActivityBalances,
  trendsHref,
  addTransactionHref,
}: BalanceWorkspaceViewProps<TEvent, TDeletePayload>): JSX.Element {
  const canWrite = useWriteAccess();
  const [updateDialogOpen, setUpdateDialogOpen] = useState(false);
  const [activeView, setActiveView] = useState<"transactions" | "activity">(
    "transactions",
  );
  const [deleteState, deleteDispatch, deletePending] = useActionState(
    deleteAction,
    {},
  );
  const entityLabel = entityName === "account" ? "Account" : "Fund";

  return (
    <ConstrainedContent maxWidth={1200}>
      <PageLayout>
        <Frame
          title="Details"
          color="info"
          headerContent={
            !canWrite ? undefined : (
              <Stack direction="row" spacing={1.25} flexWrap="wrap" useFlexGap>
                <Button
                  variant="contained"
                  onClick={() => {
                    setUpdateDialogOpen(true);
                  }}
                >
                  Edit
                </Button>
                <ConfirmActionDialog
                  trigger={(openDialog) => (
                    <Button
                      color="error"
                      variant="outlined"
                      onClick={openDialog}
                    >
                      Delete
                    </Button>
                  )}
                  title={`Delete ${entityLabel}`}
                  confirmationCopy={
                    <>
                      Are you sure you want to delete the {entityName} &quot;
                      {name}&quot;?
                    </>
                  }
                  confirmLabel="Delete"
                  confirmButtonProps={{ color: "error" }}
                  pending={deletePending}
                  errorTitle={deleteState.errorTitle}
                  unmappedErrors={deleteState.unmappedErrors}
                  onConfirm={() => {
                    startTransition(() => {
                      deleteDispatch(deletePayload);
                    });
                  }}
                />
              </Stack>
            )
          }
        >
          <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
            {summaryFields}
          </ResponsiveGrid>
        </Frame>
        <Frame title="Current Balance" color="info">
          <Box sx={{ px: { xs: 1, md: 1.5 }, py: 1 }}>
            <ResponsiveGrid minimumColumnWidth={260} spacing={3}>
              <Stack spacing={0.75}>
                <Typography variant="body2" color="text.secondary">
                  Posted Balance
                </Typography>
                <Typography
                  component="p"
                  variant="h4"
                  sx={{ overflowWrap: "anywhere" }}
                >
                  {formatCurrency(postedBalance)}
                </Typography>
              </Stack>
              <Stack spacing={0.75} justifyContent="center">
                <Typography variant="body2" color="text.secondary">
                  Pending
                </Typography>
                <Typography
                  component="p"
                  variant="h5"
                  sx={{ overflowWrap: "anywhere" }}
                >
                  {formatSignedCurrency(pendingAmount)}
                </Typography>
              </Stack>
            </ResponsiveGrid>
          </Box>
        </Frame>
        <Box>
          <Tabs
            value={activeView}
            onChange={(_, value: "transactions" | "activity") => {
              setActiveView(value);
            }}
            aria-label={`${entityLabel} workspace views`}
            sx={{ mb: 2 }}
          >
            <Tab
              value="transactions"
              label="Transactions"
              id={`${entityName}-transactions-tab`}
              aria-controls={`${entityName}-transactions-panel`}
            />
            <Tab
              value="activity"
              label="Recent Activity"
              id={`${entityName}-activity-tab`}
              aria-controls={`${entityName}-activity-panel`}
            />
          </Tabs>
          <Box
            role="tabpanel"
            id={`${entityName}-transactions-panel`}
            aria-labelledby={`${entityName}-transactions-tab`}
            hidden={activeView !== "transactions"}
          >
            {activeView === "transactions" ? (
              <BalanceEventsFrame<TEvent>
                data={recentBalanceEvents}
                totalCount={recentBalanceEventCount}
                addTransactionHref={addTransactionHref}
                sortEnum={balanceEventSort}
                emptyDescription={emptyBalanceEventsDescription}
              />
            ) : null}
          </Box>
          <Box
            role="tabpanel"
            id={`${entityName}-activity-panel`}
            aria-labelledby={`${entityName}-activity-tab`}
            hidden={activeView !== "activity"}
          >
            {activeView === "activity" ? (
              <RecentBalanceActivity
                summaryFirst
                title="Balance Activity"
                totals={activityTotals}
                dailyBalances={recentActivityBalances}
                {...(activityTotals.openingBalance === null ||
                activityTotals.openingBalance === undefined
                  ? {}
                  : { periodOpeningBalance: activityTotals.openingBalance })}
                trendsHref={trendsHref}
              />
            ) : null}
          </Box>
        </Box>
        {canWrite && updateDialogOpen
          ? renderUpdateForm(() => {
              setUpdateDialogOpen(false);
            })
          : null}
      </PageLayout>
    </ConstrainedContent>
  );
};

export default BalanceWorkspaceView;
