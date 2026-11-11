"use client";

import EntityDetailActions from "@/balance-events/EntityDetailActions";
import type { FundWithBalance } from "@/funds/types";
import type { JSX } from "react";
import UpdateFundForm from "@/funds/workspace/UpdateFundForm";
import deleteFund from "@/funds/workspace/deleteFund";

/**
 * Shows the actions for one fund.
 */
const FundDetailActions = function ({
  fund,
  redirectUrl,
  deleteRedirectUrl,
}: {
  readonly fund: FundWithBalance;
  readonly redirectUrl: string;
  readonly deleteRedirectUrl: string;
}): JSX.Element {
  return (
    <EntityDetailActions
      entityName="fund"
      name={fund.name}
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
