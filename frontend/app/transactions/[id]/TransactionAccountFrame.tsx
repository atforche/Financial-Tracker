import {
  type TransactionAccount,
  TransactionAccountType,
} from "@/data/transactionTypes";
import Caption from "@/framework/view/Caption";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import TransactionAccountBalanceFrame from "@/app/transactions/[id]/TransactionAccountBalanceFrame";
import dayjs from "dayjs";

/**
 * Props for the TransactionAccountFrame component.
 */
interface TransactionAccountFrameProps {
  readonly transactionAccount: TransactionAccount;
}

/**
 * Component that presents the user with a frame showing Transaction Account details.
 */
const TransactionAccountFrame = function ({
  transactionAccount,
}: TransactionAccountFrameProps): JSX.Element {
  return (
    <CaptionedFrame
      caption={
        transactionAccount.type === TransactionAccountType.Credit
          ? "Credit Account"
          : "Debit Account"
      }
      minWidth={500}
      maxWidth={null}
    >
      <CaptionedValue
        caption="Account"
        value={transactionAccount.accountName}
      />
      <CaptionedValue
        caption="Posted On"
        value={
          transactionAccount.postedDate === null
            ? ""
            : dayjs(transactionAccount.postedDate).format("MMMM DD, YYYY")
        }
      />
      <br />
      <Caption caption="Account Balance" />
      <TransactionAccountBalanceFrame transactionAccount={transactionAccount} />
    </CaptionedFrame>
  );
};

export default TransactionAccountFrame;
