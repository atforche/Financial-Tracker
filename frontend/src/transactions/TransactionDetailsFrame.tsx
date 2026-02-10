import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import CaptionedValue from "@framework/dialog/CaptionedValue";
import type { JSX } from "react";
import type { Transaction } from "@transactions/ApiTypes";
import dayjs from "dayjs";
import formatCurrency from "@framework/formatCurrency";

/**
 * Props for the TransactionDetailsFrame component.
 */
interface TransactionDetailsFrameProps {
  readonly transaction: Transaction;
}

/**
 * Component that prevents the user with a frame showing Transaction details.
 * @param props - Props for the TransactionDetailsFrame component.
 * @returns JSX element representing the TransactionDetailsFrame.
 */
const TransactionDetailsFrame = function ({
  transaction,
}: TransactionDetailsFrameProps): JSX.Element {
  return (
    <CaptionedFrame caption="Details">
      <CaptionedValue
        caption="Date"
        value={dayjs(transaction.date).format("MMMM DD, YYYY")}
      />
      <CaptionedValue
        caption="Accounting Period"
        value={transaction.accountingPeriodName}
      />
      <br />
      <CaptionedValue caption="Location" value={transaction.location} />
      <CaptionedValue caption="Description" value={transaction.description} />
      <CaptionedValue
        caption="Amount"
        value={formatCurrency(transaction.amount)}
      />
    </CaptionedFrame>
  );
};

export default TransactionDetailsFrame;
