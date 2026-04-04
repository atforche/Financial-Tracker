import { Checkbox, FormControlLabel, Stack } from "@mui/material";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, useState } from "react";
import dayjs, { type Dayjs } from "dayjs";
import AccountEntryField from "@/framework/forms/AccountEntryField";
import type { AccountIdentifier } from "@/data/accountTypes";
import DateEntryField from "@/framework/forms/DateEntryField";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";

/**
 * Props for the UpdateSingleTransactionAccountFrame component.
 */
interface UpdateSingleTransactionAccountFrameProps {
  readonly isDebit: boolean;
  readonly account: AccountIdentifier;
  readonly funds: FundIdentifier[];
  readonly fundAmounts: FundAmount[];
  readonly setFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly transactionDate?: string | null;
  readonly postedDate?: Dayjs | null;
  readonly setPostedDate?: ((date: Dayjs | null) => void) | null;
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
  transactionDate = null,
  postedDate = null,
  setPostedDate = null,
}: UpdateSingleTransactionAccountFrameProps): JSX.Element {
  const [postImmediately, setPostImmediately] = useState(false);
  const defaultDate = dayjs(transactionDate);

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
      {setPostedDate !== null && (
        <Stack direction="row">
          <FormControlLabel
            control={
              <Checkbox
                checked={postImmediately}
                onChange={(e): void => {
                  const { checked } = e.target;
                  setPostImmediately(checked);
                  setPostedDate(checked ? defaultDate : null);
                }}
              />
            }
            label="Post Immediately"
          />
          <DateEntryField
            label="Posted Date"
            value={postedDate}
            setValue={setPostedDate}
            disabled={!postImmediately}
          />
        </Stack>
      )}
    </Stack>
  );
};

export default UpdateSingleTransactionAccountFrame;
