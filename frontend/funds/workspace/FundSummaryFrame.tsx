"use client";

import type { JSX, ReactNode } from "react";
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
    <Frame title="Details" color="info" headerContent={headerContent}>
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
      </ResponsiveGrid>
    </Frame>
  );
};

export default FundSummaryFrame;
