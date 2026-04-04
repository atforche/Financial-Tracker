import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import type { Dayjs } from "dayjs";
import type { JSX } from "react";
import type { Transaction } from "@/data/transactionTypes";
import UpdateSingleTransactionAccountFrame from "@/app/transactions/[id]/update/UpdateSingleTransactionAccountFrame";
import UpdateTransferTransactionAccountFrame from "@/app/transactions/[id]/update/UpdateTransferTransactionAccountFrame";

/**
 * Props for the UpdateTransactionAccountFrame component.
 */
interface UpdateTransactionAccountFrameProps {
  readonly transaction: Transaction;
  readonly funds: FundIdentifier[];
  readonly debitFundAmounts: FundAmount[];
  readonly setDebitFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly creditFundAmounts: FundAmount[];
  readonly setCreditFundAmounts: (fundAmounts: FundAmount[]) => void;
  readonly debitPostedDate: Dayjs | null;
  readonly setDebitPostedDate: (date: Dayjs | null) => void;
  readonly creditPostedDate: Dayjs | null;
  readonly setCreditPostedDate: (date: Dayjs | null) => void;
}

/**
 * Component that displays a frame for updating the transaction account(s) for a transaction.
 */
const UpdateTransactionAccountFrame = function ({
  transaction,
  funds,
  debitFundAmounts,
  setDebitFundAmounts,
  creditFundAmounts,
  setCreditFundAmounts,
  debitPostedDate,
  setDebitPostedDate,
  creditPostedDate,
  setCreditPostedDate,
}: UpdateTransactionAccountFrameProps): JSX.Element {
  const debitAccount =
    transaction.debitAccount !== null &&
    typeof transaction.debitAccount !== "undefined"
      ? {
          id: transaction.debitAccount.accountId,
          name: transaction.debitAccount.accountName,
        }
      : null;
  const creditAccount =
    transaction.creditAccount !== null &&
    typeof transaction.creditAccount !== "undefined"
      ? {
          id: transaction.creditAccount.accountId,
          name: transaction.creditAccount.accountName,
        }
      : null;

  if (debitAccount !== null && creditAccount !== null) {
    return (
      <UpdateTransferTransactionAccountFrame
        debitAccount={debitAccount}
        creditAccount={creditAccount}
        funds={funds}
        debitFundAmounts={debitFundAmounts}
        setDebitFundAmounts={setDebitFundAmounts}
        creditFundAmounts={creditFundAmounts}
        setCreditFundAmounts={setCreditFundAmounts}
      />
    );
  }
  if (debitAccount !== null) {
    return (
      <UpdateSingleTransactionAccountFrame
        isDebit
        account={debitAccount}
        funds={funds}
        fundAmounts={debitFundAmounts}
        setFundAmounts={setDebitFundAmounts}
        transactionDate={transaction.date}
        postedDate={debitPostedDate}
        setPostedDate={setDebitPostedDate}
      />
    );
  }
  if (creditAccount !== null) {
    return (
      <UpdateSingleTransactionAccountFrame
        isDebit={false}
        account={creditAccount}
        funds={funds}
        fundAmounts={creditFundAmounts}
        setFundAmounts={setCreditFundAmounts}
        transactionDate={transaction.date}
        postedDate={creditPostedDate}
        setPostedDate={setCreditPostedDate}
      />
    );
  }
  throw new Error(
    "Transaction must have at least a debit account or a credit account",
  );
};

export default UpdateTransactionAccountFrame;
