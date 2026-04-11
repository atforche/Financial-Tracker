import {
  type CreateTransactionFormSearchParams,
  getDefaultCreditAccount,
  getDefaultDebitAccount,
} from "@/app/transactions/create/createTransactionFormSearchParams";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { Stack, ToggleButton, ToggleButtonGroup } from "@mui/material";
import type { AccountIdentifier } from "@/data/accountTypes";
import CreateSingleTransactionAccountFrame from "@/app/transactions/create/CreateSingleTransactionAccountFrame";
import CreateTransferTransactionAccountFrame from "@/app/transactions/create/CreateTransferTransactionAccountFrame";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import ToggleState from "@/app/transactions/create/toggleState";
import tryParseEnum from "@/data/tryParseEnum";

/**
 * Props for the CreateTransactionAccountFrame component.
 */
interface CreateTransactionAccountFrameProps {
  readonly searchParams: CreateTransactionFormSearchParams;
  readonly accounts: AccountIdentifier[];
  readonly funds: FundIdentifier[];
  readonly toggleState: ToggleState;
  readonly setToggleState: (toggleState: ToggleState) => void;
  readonly debitAccount: AccountIdentifier | null;
  readonly setDebitAccount: (account: AccountIdentifier | null) => void;
  readonly debitFundAmounts: FundAmount[];
  readonly setDebitFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly creditAccount: AccountIdentifier | null;
  readonly setCreditAccount: (account: AccountIdentifier | null) => void;
  readonly creditFundAmounts: FundAmount[];
  readonly setCreditFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly date: Dayjs | null;
  readonly debitPostedDate: Dayjs | null;
  readonly setDebitPostedDate: (date: Dayjs | null) => void;
  readonly creditPostedDate: Dayjs | null;
  readonly setCreditPostedDate: (date: Dayjs | null) => void;
}

/**
 * Component that displays a frame for creating the transaction account(s) for a transaction.
 */
const CreateTransactionAccountFrame = function ({
  searchParams,
  accounts,
  funds,
  toggleState,
  setToggleState,
  debitAccount,
  setDebitAccount,
  debitFundAmounts,
  setDebitFundAmounts,
  creditAccount,
  setCreditAccount,
  creditFundAmounts,
  setCreditFundAmounts,
  date,
  debitPostedDate,
  setDebitPostedDate,
  creditPostedDate,
  setCreditPostedDate,
}: CreateTransactionAccountFrameProps): JSX.Element {
  const resetToInitialState = function (): void {
    setDebitAccount(getDefaultDebitAccount(accounts, searchParams));
    setDebitFundAmounts([]);
    setCreditAccount(getDefaultCreditAccount(accounts, searchParams));
    setCreditFundAmounts([]);
    setDebitPostedDate(null);
    setCreditPostedDate(null);
  };

  return (
    <Stack spacing={2}>
      <ToggleButtonGroup
        value={toggleState}
        exclusive
        onChange={(_, value): void => {
          if (typeof value !== "string") {
            return;
          }
          const parsedValue = tryParseEnum(ToggleState, value);
          if (parsedValue !== null) {
            setToggleState(parsedValue);
            resetToInitialState();
          }
        }}
      >
        <ToggleButton
          value={ToggleState.Debit}
          disabled={
            typeof searchParams.creditAccountId !== "undefined" ||
            typeof searchParams.creditFundId !== "undefined"
          }
        >
          Debit
        </ToggleButton>
        <ToggleButton
          value={ToggleState.Credit}
          disabled={
            typeof searchParams.debitAccountId !== "undefined" ||
            typeof searchParams.debitFundId !== "undefined"
          }
        >
          Credit
        </ToggleButton>
        <ToggleButton value={ToggleState.Transfer}>Transfer</ToggleButton>
      </ToggleButtonGroup>
      {toggleState === ToggleState.Debit && (
        <CreateSingleTransactionAccountFrame
          isDebit
          searchParams={searchParams}
          accounts={accounts}
          funds={funds}
          account={debitAccount}
          setAccount={setDebitAccount}
          fundAmounts={debitFundAmounts}
          setFundAmounts={setDebitFundAmounts}
          date={date}
          postedDate={debitPostedDate}
          setPostedDate={setDebitPostedDate}
        />
      )}
      {toggleState === ToggleState.Credit && (
        <CreateSingleTransactionAccountFrame
          isDebit={false}
          searchParams={searchParams}
          accounts={accounts}
          funds={funds}
          account={creditAccount}
          setAccount={setCreditAccount}
          fundAmounts={creditFundAmounts}
          setFundAmounts={setCreditFundAmounts}
          date={date}
          postedDate={creditPostedDate}
          setPostedDate={setCreditPostedDate}
        />
      )}
      {toggleState === ToggleState.Transfer && (
        <CreateTransferTransactionAccountFrame
          searchParams={searchParams}
          accounts={accounts}
          funds={funds}
          debitAccount={debitAccount}
          setDebitAccount={setDebitAccount}
          debitFundAmounts={debitFundAmounts}
          setDebitFundAmounts={setDebitFundAmounts}
          creditAccount={creditAccount}
          setCreditAccount={setCreditAccount}
          creditFundAmounts={creditFundAmounts}
          setCreditFundAmounts={setCreditFundAmounts}
        />
      )}
    </Stack>
  );
};

export default CreateTransactionAccountFrame;
