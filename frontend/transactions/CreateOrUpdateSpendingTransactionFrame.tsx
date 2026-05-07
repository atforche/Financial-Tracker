import type { Fund, FundAmount } from "@/funds/types";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import FundAssignmentEntryFrame from "@/funds/FundAssignmentEntryFrame";
import type { JSX } from "react";

/**
 * Props for the CreateOrUpdateSpendingTransactionFrame component.
 */
interface CreateOrUpdateSpendingTransactionFrameProps {
  readonly funds: Fund[];
  readonly amount: number | null;
  readonly spendingFundAssignments: FundAmount[];
  readonly setSpendingFundAssignments: (
    spendingFundAssignments: FundAmount[],
  ) => void;
}

/**
 * Component that displays the frame for creating or updating a spending transaction.
 */
const CreateOrUpdateSpendingTransactionFrame = function ({
  funds,
  amount,
  spendingFundAssignments,
  setSpendingFundAssignments,
}: CreateOrUpdateSpendingTransactionFrameProps): JSX.Element {
  return (
    <CaptionedFrame caption="Spending Transaction Details">
      <FundAssignmentEntryFrame
        label="Fund Assignments"
        funds={funds}
        totalAmountToAssign={amount}
        value={spendingFundAssignments}
        setValue={setSpendingFundAssignments}
      />
    </CaptionedFrame>
  );
};

export default CreateOrUpdateSpendingTransactionFrame;
