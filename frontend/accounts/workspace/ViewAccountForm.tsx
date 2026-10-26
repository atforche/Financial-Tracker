"use client";

import {
  type AccountBalanceEvent,
  AccountBalanceEventSort,
  type AccountWithBalance,
} from "@/accounts/types";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import type { BalanceEventTotals } from "@/balance-events/types";
import type { BalanceTrendDateSummary } from "@/framework/charts/balanceTrendHelpers";
import BalanceWorkspaceView from "@/balance-events/BalanceWorkspaceView";
import type { JSX } from "react";
import type { Route } from "next";
import StringEntryField from "@/framework/forms/StringEntryField";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";
import deleteAccount from "@/accounts/workspace/deleteAccount";

/**
 * Props for the ViewAccountForm component.
 */
interface ViewAccountFormProps {
  readonly account: AccountWithBalance;
  readonly financialInstitutions: readonly string[];
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
  readonly recentBalanceEvents: AccountBalanceEvent[];
  readonly recentBalanceEventCount: number;
  readonly activityTotals: BalanceEventTotals;
  readonly recentActivityBalances: readonly BalanceTrendDateSummary[];
  readonly trendsHref: Route;
  readonly addTransactionHref: string;
}

/**
 * Displays the read-only account workspace view for a selected account.
 */
const ViewAccountForm = function ({
  account,
  financialInstitutions,
  redirectUrl,
  deleteRedirectUrl,
  ...viewProps
}: ViewAccountFormProps): JSX.Element {
  return (
    <BalanceWorkspaceView
      {...viewProps}
      entityName="account"
      name={account.name}
      summaryFields={
        <>
          <StringEntryField
            label="Name"
            value={account.name}
            setValue={null}
            errorMessage={null}
          />
          <StringEntryField
            label="Financial Institution"
            value={account.financialInstitution}
            setValue={null}
            errorMessage={null}
          />
          <AccountTypeEntryField
            value={account.type}
            setValue={null}
            errorMessage={null}
          />
        </>
      }
      postedBalance={account.currentBalance.postedBalance}
      pendingAmount={account.pendingAmount}
      deleteAction={deleteAccount}
      deletePayload={{ accountId: account.id, redirectUrl: deleteRedirectUrl }}
      renderUpdateForm={(onClose) => (
        <UpdateAccountForm
          account={account}
          financialInstitutions={financialInstitutions}
          redirectUrl={redirectUrl}
          onClose={onClose}
        />
      )}
      balanceEventSort={AccountBalanceEventSort}
      emptyBalanceEventsDescription="Create or post a transaction for this account to start building its balance history."
    />
  );
};

export default ViewAccountForm;
