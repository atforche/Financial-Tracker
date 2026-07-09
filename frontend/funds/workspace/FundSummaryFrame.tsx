"use client";

import type { JSX, ReactNode } from "react";
import { Box } from "@mui/material";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import Frame from "@/framework/view/Frame";
import type { Fund } from "@/funds/types";
import StringEntryField from "@/framework/forms/StringEntryField";

/**
 * Props for the FundSummaryFrame component.
 */
interface FundSummaryFrameProps {
  readonly fund: Fund;
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
    fund.currentBalance.pendingAmountAssigned -
    fund.currentBalance.pendingAmountSpent;

  return (
    <Box sx={{ maxWidth: 1200, width: "100%" }}>
      <Frame title="Fund Summary" color="info" headerContent={headerContent}>
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns:
              "repeat(auto-fit, minmax(min(100%, 220px), 1fr))",
          }}
        >
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
        </Box>
      </Frame>
    </Box>
  );
};

export default FundSummaryFrame;
