import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import AccountEntryField from "@/framework/forms/AccountEntryField";
import type { AccountIdentifier } from "@/data/accountTypes";
import { ArrowRightAlt } from "@mui/icons-material";
import type { CreateTransactionFormSearchParams } from "@/app/transactions/create/createTransactionFormSearchParams";
import FundAmountCollectionEntryFrame from "@/framework/forms/FundAmountCollectionEntryFrame";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the CreateTransferTransactionAccountFrame component.
 */
interface CreateTransferTransactionAccountFrameProps {
  readonly searchParams: CreateTransactionFormSearchParams;
  readonly accounts: AccountIdentifier[];
  readonly funds: FundIdentifier[];
  readonly debitAccount: AccountIdentifier | null;
  readonly setDebitAccount: (account: AccountIdentifier | null) => void;
  readonly debitFundAmounts: FundAmount[];
  readonly setDebitFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly creditAccount: AccountIdentifier | null;
  readonly setCreditAccount: (account: AccountIdentifier | null) => void;
  readonly creditFundAmounts: FundAmount[];
  readonly setCreditFundAmounts: (fundAmounts: FundAmount[]) => void;
}

/**
 * Component that displays a frame for creating two transaction accounts for a transfer transaction.
 */
const CreateTransferTransactionAccountFrame = function ({
  searchParams,
  accounts,
  funds,
  debitAccount,
  setDebitAccount,
  debitFundAmounts,
  setDebitFundAmounts,
  creditAccount,
  setCreditAccount,
  creditFundAmounts,
  setCreditFundAmounts,
}: CreateTransferTransactionAccountFrameProps): JSX.Element {
  return (
    <Stack direction="row" spacing={2}>
      <Stack spacing={2} sx={{ minWidth: 500 }}>
        <AccountEntryField
          label="Debit From"
          options={accounts}
          value={debitAccount}
          setValue={
            typeof searchParams.debitAccountId !== "undefined"
              ? null
              : (value): void => {
                  setDebitAccount(value);
                }
          }
        />
        <FundAmountCollectionEntryFrame
          label="Funds to Debit"
          funds={funds}
          value={debitFundAmounts}
          setValue={(value): void => {
            setDebitFundAmounts(value);
          }}
          lockedFundIds={
            typeof searchParams.debitFundId !== "undefined"
              ? [searchParams.debitFundId]
              : []
          }
        />
      </Stack>
      <ArrowRightAlt
        sx={{ alignSelf: "center", color: "rgba(0, 0, 0, 0.6)", fontSize: 40 }}
      />
      <Stack spacing={2} sx={{ minWidth: 500 }}>
        <AccountEntryField
          label="Credit To"
          options={accounts}
          value={creditAccount}
          setValue={
            typeof searchParams.creditAccountId !== "undefined"
              ? null
              : (value): void => {
                  setCreditAccount(value);
                }
          }
        />
        <FundAmountCollectionEntryFrame
          label="Funds to Credit"
          funds={funds}
          value={creditFundAmounts}
          setValue={(value): void => {
            setCreditFundAmounts(value);
          }}
          lockedFundIds={
            typeof searchParams.creditFundId !== "undefined"
              ? [searchParams.creditFundId]
              : []
          }
        />
      </Stack>
    </Stack>
  );
};

export default CreateTransferTransactionAccountFrame;
