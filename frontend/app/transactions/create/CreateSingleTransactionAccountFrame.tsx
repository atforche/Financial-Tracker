import { Checkbox, FormControlLabel, Stack } from "@mui/material";
import {
  type CreateTransactionFormSearchParams,
  getDefaultCreditAccount,
  getDefaultDebitAccount,
} from "@/app/transactions/create/createTransactionFormSearchParams";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, useState } from "react";
import AccountEntryField from "@/framework/forms/AccountEntryField";
import type { AccountIdentifier } from "@/data/accountTypes";
import DateEntryField from "@/framework/forms/DateEntryField";
import type { Dayjs } from "dayjs";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";

/**
 * Props for the CreateSingleTransactionAccountFrame component.
 */
interface CreateSingleTransactionAccountFrameProps {
  readonly isDebit: boolean;
  readonly searchParams: CreateTransactionFormSearchParams;
  readonly accounts: AccountIdentifier[];
  readonly funds: FundIdentifier[];
  readonly account: AccountIdentifier | null;
  readonly setAccount: (account: AccountIdentifier | null) => void;
  readonly fundAmounts: FundAmount[];
  readonly setFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly date?: Dayjs | null;
  readonly postedDate?: Dayjs | null;
  readonly setPostedDate?: ((date: Dayjs | null) => void) | null;
}

/**
 * Component that displays a frame for creating a single transaction account for a debit or credit transaction.
 */
const CreateSingleTransactionAccountFrame = function ({
  isDebit,
  searchParams,
  accounts,
  funds,
  account,
  setAccount,
  fundAmounts,
  setFundAmounts,
  date = null,
  postedDate = null,
  setPostedDate = null,
}: CreateSingleTransactionAccountFrameProps): JSX.Element {
  const [postImmediately, setPostImmediately] = useState(false);

  const requiredAccount = isDebit
    ? getDefaultDebitAccount(accounts, searchParams)
    : getDefaultCreditAccount(accounts, searchParams);

  return (
    <Stack spacing={2} sx={{ maxWidth: 500, minWidth: 500 }}>
      <AccountEntryField
        label={isDebit ? "Debit From" : "Credit To"}
        options={accounts}
        value={account}
        setValue={
          requiredAccount !== null
            ? null
            : (value): void => {
                setAccount(value);
              }
        }
      />
      <FundAmountCollectionEntryFrame
        label={isDebit ? "Funds to Debit" : "Funds to Credit"}
        funds={funds}
        value={fundAmounts}
        setValue={(value): void => {
          setFundAmounts(value);
        }}
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
                  setPostedDate(checked ? date : null);
                }}
              />
            }
            label="Post Immediately"
          />
          <DateEntryField
            label="Posted Date"
            value={postImmediately ? (postedDate ?? date) : null}
            setValue={setPostedDate}
            disabled={!postImmediately}
          />
        </Stack>
      )}
    </Stack>
  );
};

export default CreateSingleTransactionAccountFrame;
