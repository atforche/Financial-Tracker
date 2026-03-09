import AccountingPeriodListFrame from "@/app/accounting-periods/AccountingPeriodListFrame";
import type { JSX } from "react";

/**
 * Component that displays the Accounting Period view.
 */
const AccountingPeriodView = function (): JSX.Element {
  return (
    <div>
      <AccountingPeriodListFrame />
    </div>
  );
};

export default AccountingPeriodView;
