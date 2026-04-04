import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import type { AccountIdentifier } from "@/data/accountTypes";
import { ArrowRightAlt } from "@mui/icons-material";
import CreateSingleTransactionAccountFrame from "@/app/transactions/create/CreateSingleTransactionAccountFrame";
import type { CreateTransactionFormSearchParams } from "@/app/transactions/create/createTransactionFormSearchParams";
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
      <CreateSingleTransactionAccountFrame
        isDebit
        searchParams={searchParams}
        accounts={accounts}
        funds={funds}
        account={debitAccount}
        setAccount={setDebitAccount}
        fundAmounts={debitFundAmounts}
        setFundAmounts={setDebitFundAmounts}
      />
      <ArrowRightAlt
        sx={{ alignSelf: "center", color: "rgba(0, 0, 0, 0.6)", fontSize: 40 }}
      />
      <CreateSingleTransactionAccountFrame
        isDebit={false}
        searchParams={searchParams}
        accounts={accounts}
        funds={funds}
        account={creditAccount}
        setAccount={setCreditAccount}
        fundAmounts={creditFundAmounts}
        setFundAmounts={setCreditFundAmounts}
      />
    </Stack>
  );
};

export default CreateTransferTransactionAccountFrame;
