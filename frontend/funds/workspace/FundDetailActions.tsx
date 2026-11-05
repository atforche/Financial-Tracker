"use client";

import EntityDetailActions from "@/balance-events/EntityDetailActions";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import type { Route } from "next";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";
import deleteFund from "@/funds/workspace/deleteFund";

/**
 * Shows the actions for one fund.
 */
const FundDetailActions = function ({
  fund,
  addTransactionHref,
  trendsHref,
  redirectUrl,
  deleteRedirectUrl,
}: {
  readonly fund: FundWithBalance;
  readonly addTransactionHref: string;
  readonly trendsHref: Route;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
}): JSX.Element {
  return (
    <EntityDetailActions
      entityName="fund"
      name={fund.name}
      addTransactionHref={addTransactionHref}
      trendsHref={trendsHref}
      deleteAction={deleteFund}
      deletePayload={{ fundId: fund.id, redirectUrl: deleteRedirectUrl }}
      renderUpdateForm={(onClose) => (
        <UpdateFundForm
          fund={fund}
          redirectUrl={redirectUrl}
          onClose={onClose}
        />
      )}
    />
  );
};

export default FundDetailActions;
