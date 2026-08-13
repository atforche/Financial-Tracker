"use client";

import type { JSX, ReactNode } from "react";
import type { AccountingPeriodWithBalance } from "@/accounting-periods/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Frame from "@/framework/view/Frame";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the AccountingPeriodSummaryFrame component.
 */
interface AccountingPeriodSummaryFrameProps {
  readonly accountingPeriod: AccountingPeriodWithBalance;
  readonly headerContent?: ReactNode;
}

/**
 * Displays the primary accounting period facts and lifecycle actions.
 */
const AccountingPeriodSummaryFrame = function ({
  accountingPeriod,
  headerContent = null,
}: AccountingPeriodSummaryFrameProps): JSX.Element {
  return (
    <Frame
      title="Accounting Period Summary"
      color="info"
      headerContent={headerContent}
    >
      <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
        <StringEntryField
          label="Period"
          value={accountingPeriod.name}
          setValue={null}
        />
        <StringEntryField
          label="Status"
          value={accountingPeriod.isOpen ? "Open" : "Closed"}
          setValue={null}
        />
        <CurrencyEntryField
          label="Opening Balance"
          value={accountingPeriod.openingBalance}
          setValue={null}
        />
        <CurrencyEntryField
          label="Closing Balance"
          value={accountingPeriod.closingBalance}
          setValue={null}
        />
      </ResponsiveGrid>
    </Frame>
  );
};
export default AccountingPeriodSummaryFrame;
