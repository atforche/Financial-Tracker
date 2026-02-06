import {
  type TransactionAccount,
  TransactionAccountType,
} from "@transactions/ApiTypes";
import Caption from "@framework/dialog/Caption";
import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import CaptionedValue from "@framework/dialog/CaptionedValue";
import type { JSX } from "react";
import TransactionAccountBalanceFrame from "@transactions/TransactionAccountBalanceFrame";
import dayjs from "dayjs";

/**
 * Props for the TransactionAccountFrame component.
 */
interface TransactionAccountFrameProps {
  readonly transactionAccount: TransactionAccount;
  readonly transactionAccountType: TransactionAccountType;
}

/**
 * Component that presents the user with a frame showing Transaction Account details.
 * @param props - Props for the TransactionAccountFrame component.
 * @returns JSX element representing the TransactionAccountFrame.
 */
const TransactionAccountFrame = function ({
  transactionAccount,
  transactionAccountType,
}: TransactionAccountFrameProps): JSX.Element {
  return (
    <CaptionedFrame
      caption={
        transactionAccountType === TransactionAccountType.Credit
          ? "Credit Account"
          : "Debit Account"
      }
      minWidth={500}
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
      <TransactionAccountBalanceFrame
        transactionAccount={transactionAccount}
        transactionAccountType={transactionAccountType}
      />
    </CaptionedFrame>
  );
};

export default TransactionAccountFrame;
