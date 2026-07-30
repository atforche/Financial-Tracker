"use client";

import type { JSX, ReactNode } from "react";
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
  return (
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
          value={fund.currentBalance.balanceIncludingPending}
          setValue={null}
          errorMessage={null}
        />
      </ResponsiveGrid>
    </Frame>
  );
};

export default FundSummaryFrame;
