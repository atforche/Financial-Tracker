"use client";

import type { JSX, ReactNode } from "react";
import ConstrainedContent from "@/framework/view/ConstrainedContent";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Frame from "@/framework/view/Frame";
import type { FundWithBalance } from "@/funds/types";
import ResponsiveGrid from "@/framework/view/ResponsiveGrid";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the FundSummaryFrame component.
 */
interface FundSummaryFrameProps {
  readonly fund: FundWithBalance;
  readonly headerContent?: ReactNode;
}

/**
 * Displays the primary fund summary for the workspace detail view.
 */
const FundSummaryFrame = function ({
  fund,
  headerContent,
}: FundSummaryFrameProps): JSX.Element {
  const balanceIncludingPending =
    fund.currentBalance.postedBalance +
    fund.currentBalance.pendingCreditAmount -
    fund.currentBalance.pendingDebitAmount;

  return (
    <ConstrainedContent maxWidth={1200}>
      <Frame title="Fund Summary" color="info" headerContent={headerContent}>
        <ResponsiveGrid minimumColumnWidth={220} spacing={2}>
          <StringEntryField
            label="Name"
            value={fund.name}
            setValue={null}
            errorMessage={null}
          />
          <StringEntryField
            label="Description"
            value={fund.description}
            setValue={null}
            errorMessage={null}
          />
          <CurrencyEntryField
            label="Current Balance"
            value={fund.currentBalance.postedBalance}
            setValue={null}
            errorMessage={null}
          />
          <CurrencyEntryField
            label="Balance Including Pending"
            value={balanceIncludingPending}
            setValue={null}
            errorMessage={null}
          />
        </ResponsiveGrid>
      </Frame>
    </ConstrainedContent>
  );
};

export default FundSummaryFrame;
