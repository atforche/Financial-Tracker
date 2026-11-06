"use client";

import type { AccountWithBalance } from "@/accounts/types";
import EntityDetailActions from "@/balance-events/EntityDetailActions";
import type { JSX } from "react";
import type { Route } from "next";
import UpdateAccountForm from "@/accounts/workspace/UpdateAccountForm";
import deleteAccount from "@/accounts/workspace/deleteAccount";

/**
 * Shows the actions for one account.
 */
const AccountDetailActions = function ({
  account,
  addTransactionHref,
  trendsHref,
  redirectUrl,
  deleteRedirectUrl,
}: {
  readonly account: AccountWithBalance;
  readonly addTransactionHref: string;
  readonly trendsHref: Route;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
}): JSX.Element {
  return (
    <EntityDetailActions
      entityName="account"
      name={account.name}
      addTransactionHref={addTransactionHref}
      trendsHref={trendsHref}
      deleteAction={deleteAccount}
      deletePayload={{ accountId: account.id, redirectUrl: deleteRedirectUrl }}
      renderUpdateForm={(onClose) => (
        <UpdateAccountForm
          account={account}
          redirectUrl={redirectUrl}
          onClose={onClose}
        />
      )}
    />
  );
};

export default AccountDetailActions;
