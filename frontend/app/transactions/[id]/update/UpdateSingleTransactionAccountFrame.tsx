import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import AccountEntryField from "@/framework/forms/AccountEntryField";
import type { AccountIdentifier } from "@/data/accountTypes";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the UpdateSingleTransactionAccountFrame component.
 */
interface UpdateSingleTransactionAccountFrameProps {
  readonly isDebit: boolean;
  readonly account: AccountIdentifier;
  readonly funds: FundIdentifier[];
  readonly fundAmounts: FundAmount[];
  readonly setFundAmounts: (fundAmounts: FundAmount[]) => void;
}

/**
 * Component that displays a frame for updating a single transaction account for a debit or credit transaction.
 */
const UpdateSingleTransactionAccountFrame = function ({
  isDebit,
  account,
  funds,
  fundAmounts,
  setFundAmounts,
}: UpdateSingleTransactionAccountFrameProps): JSX.Element {
  return (
    <Stack spacing={2} sx={{ maxWidth: 500, minWidth: 500 }}>
      <AccountEntryField
        label={isDebit ? "Debit From" : "Credit To"}
        options={[account]}
        value={account}
        setValue={null}
      />
      <FundAmountCollectionEntryFrame
        label={isDebit ? "Funds to Debit" : "Funds to Credit"}
        funds={funds}
        value={fundAmounts}
        setValue={(value): void => {
          setFundAmounts(value);
        }}
        lockedFundIds={[]}
      />
    </Stack>
  );
};

export default UpdateSingleTransactionAccountFrame;
