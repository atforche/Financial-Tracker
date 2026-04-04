import {
  type CreateTransactionFormSearchParams,
  getDefaultCreditAccount,
  getDefaultCreditFundAmount,
  getDefaultDebitAccount,
  getDefaultDebitFundAmount,
} from "@/app/transactions/create/createTransactionFormSearchParams";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import AccountEntryField from "@/framework/forms/AccountEntryField";
import type { AccountIdentifier } from "@/data/accountTypes";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";

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
}: CreateSingleTransactionAccountFrameProps): JSX.Element {
  const requiredAccount = isDebit
    ? getDefaultDebitAccount(accounts, searchParams)
    : getDefaultCreditAccount(accounts, searchParams);
  const requiredFund = isDebit
    ? getDefaultDebitFundAmount(funds, searchParams).at(0)
    : getDefaultCreditFundAmount(funds, searchParams).at(0);

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
        lockedFundIds={
          typeof requiredFund !== "undefined" ? [requiredFund.fundId] : []
        }
      />
    </Stack>
  );
};

export default CreateSingleTransactionAccountFrame;
