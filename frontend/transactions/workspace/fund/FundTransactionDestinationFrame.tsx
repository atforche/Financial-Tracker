import { Box, Stack } from "@mui/material";
import type {
  Fund,
  FundBalanceEvent,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/transactions/workspace/TransactionSection";
import FundBalanceEventFrame from "@/transactions/workspace/FundBalanceEventFrame";
import type { JSX } from "react";
import TransactionSourceOrDestinationFrame from "@/transactions/workspace/TransactionSourceOrDestinationFrame";

/**
 * Props for the FundTransactionDestinationFrame component.
 */
interface FundTransactionDestinationFrameProps {
  readonly index: number;
  readonly funds: FundWithBalance[];
  readonly fund: FundBalanceEventDraft | null;
  readonly setFund: ((fund: FundBalanceEventDraft | null) => void) | null;
  readonly amount: number | null;
  readonly setAmount: ((amount: number | null) => void) | null;
  readonly filter?: ((fund: Fund) => boolean) | null;
  readonly onRemove?: (() => void) | null;
  readonly color?: FrameColor;
  readonly fundBalanceEvent?: FundBalanceEvent | null;
  readonly readOnly?: boolean;
  readonly autoFocus?: boolean;
}

/**
 * Displays a form frame for one fund transaction destination.
 */
const FundTransactionDestinationFrame = function ({
  index,
  funds,
  fund,
  setFund,
  amount,
  setAmount,
  filter = null,
  onRemove = null,
  color = "info",
  fundBalanceEvent = null,
  readOnly = false,
  autoFocus = false,
}: FundTransactionDestinationFrameProps): JSX.Element {
  return (
    <TransactionSourceOrDestinationFrame
      title={`Destination ${index + 1}`}
      onRemove={readOnly ? null : onRemove}
      color={color}
    >
      <Stack
        // Viewing keeps the name and amount on one row on phones, since neither needs an input's width
        direction={{ xs: readOnly ? "row" : "column", sm: "row" }}
        spacing={2}
        alignItems={{
          xs: readOnly ? "flex-start" : "stretch",
          sm: "flex-start",
        }}
      >
        <Box sx={{ flex: "1 1 auto", minWidth: 0 }}>
          <FundBalanceEventFrame
            funds={funds}
            fund={fund}
            setFund={readOnly ? null : setFund}
            fundFilter={filter}
            label="Destination Fund"
            balanceEvent={fundBalanceEvent}
            autoFocus={autoFocus}
          />
        </Box>
        <CurrencyEntryField
          label="Destination Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{
            width: { xs: readOnly ? "auto" : "100%", sm: 220 },
            // Keeps a viewed amount whole beside a long name or balance chips
            flexShrink: 0,
            whiteSpace: "nowrap",
          }}
        />
      </Stack>
    </TransactionSourceOrDestinationFrame>
  );
};

export default FundTransactionDestinationFrame;
