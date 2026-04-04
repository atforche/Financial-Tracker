import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import type { AccountIdentifier } from "@/data/accountTypes";
import { ArrowRightAlt } from "@mui/icons-material";
import type { JSX } from "react";
import { Stack } from "@mui/material";
import UpdateSingleTransactionAccountFrame from "@/app/transactions/[id]/update/UpdateSingleTransactionAccountFrame";

/**
 * Props for the UpdateTransferTransactionAccountFrame component.
 */
interface UpdateTransferTransactionAccountFrameProps {
  readonly debitAccount: AccountIdentifier;
  readonly creditAccount: AccountIdentifier;
  readonly funds: FundIdentifier[];
  readonly debitFundAmounts: FundAmount[];
  readonly setDebitFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly creditFundAmounts: FundAmount[];
  readonly setCreditFundAmounts: (fundAmounts: FundAmount[]) => void;
}

/**
 * Component that displays a frame for updating two transaction accounts for a transfer transaction.
 */
const UpdateTransferTransactionAccountFrame = function ({
  debitAccount,
  creditAccount,
  funds,
  debitFundAmounts,
  setDebitFundAmounts,
  creditFundAmounts,
  setCreditFundAmounts,
}: UpdateTransferTransactionAccountFrameProps): JSX.Element {
  return (
    <Stack direction="row" spacing={2}>
      <UpdateSingleTransactionAccountFrame
        isDebit
        account={debitAccount}
        funds={funds}
        fundAmounts={debitFundAmounts}
        setFundAmounts={setDebitFundAmounts}
      />
      <ArrowRightAlt
        sx={{ alignSelf: "center", color: "rgba(0, 0, 0, 0.6)", fontSize: 40 }}
      />
      <UpdateSingleTransactionAccountFrame
        isDebit={false}
        account={creditAccount}
        funds={funds}
        fundAmounts={creditFundAmounts}
        setFundAmounts={setCreditFundAmounts}
      />
    </Stack>
  );
};

export default UpdateTransferTransactionAccountFrame;
