import { Box, Stack } from "@mui/material";
import type {
  Fund,
  FundBalanceEventDraft,
  FundWithBalance,
} from "@/funds/types";
import CurrencyEntryField from "@/framework/forms/CurrencyEntryField";
import type { FrameColor } from "@/framework/view/Frame";
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
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        alignItems={{ xs: "stretch", sm: "flex-start" }}
      >
        <Box sx={{ flex: { sm: "1 1 auto" }, minWidth: 0 }}>
          <FundBalanceEventFrame
            funds={funds}
            fund={fund}
            setFund={readOnly ? null : setFund}
            fundFilter={filter}
            label="Destination Fund"
            balanceChange={amount}
            autoFocus={autoFocus}
          />
        </Box>
        <CurrencyEntryField
          label="Destination Amount"
          value={amount}
          setValue={readOnly ? null : setAmount}
          sx={{ width: { xs: "100%", sm: 220 } }}
        />
      </Stack>
    </TransactionSourceOrDestinationFrame>
  );
};

export default FundTransactionDestinationFrame;
