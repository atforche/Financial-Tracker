"use client";

import type { JSX, ReactNode } from "react";
import AccountTypeEntryField from "@/accounts/AccountTypeEntryField";
import type { AccountWithBalance } from "@/accounts/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Frame from "@/framework/view/Frame";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the AccountSummaryFrame component.
 */
interface AccountSummaryFrameProps {
  readonly account: AccountWithBalance;
  readonly headerContent?: ReactNode;
}

/**
 * Displays the primary account summary for the workspace detail view.
 */
const AccountSummaryFrame = function ({
  account,
  headerContent = null,
}: AccountSummaryFrameProps): JSX.Element {
  const pendingBalance =
    account.currentBalance.postedBalance -
    account.currentBalance.pendingDebitAmount +
    account.currentBalance.pendingCreditAmount;

  return (
    <Frame title="Account Summary" color="info" headerContent={headerContent}>
      <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
        <StringEntryField
          label="Name"
          value={account.name}
          setValue={null}
          errorMessage={null}
        />
        <AccountTypeEntryField
          label="Type"
          value={account.type}
          setValue={null}
          errorMessage={null}
        />
        <CurrencyEntryField
          label="Current Balance"
          value={account.currentBalance.postedBalance}
          setValue={null}
          errorMessage={null}
        />
        <CurrencyEntryField
          label="Balance Including Pending"
          value={pendingBalance}
          setValue={null}
          errorMessage={null}
        />
      </ResponsiveGrid>
    </Frame>
  );
};

export default AccountSummaryFrame;
