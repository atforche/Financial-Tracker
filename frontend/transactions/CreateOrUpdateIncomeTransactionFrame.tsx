import type { Fund, FundAmount } from "@/funds/types";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import FundAssignmentEntryFrame from "@/funds/FundAssignmentEntryFrame";
import type { JSX } from "react";

/**
 * Props for the CreateOrUpdateIncomeTransactionFrame component.
 */
interface CreateOrUpdateIncomeTransactionFrameProps {
  readonly funds: Fund[];
  readonly amount: number | null;
  readonly incomeFundAssignments: FundAmount[];
  readonly setIncomeFundAssignments: (
    incomeFundAssignments: FundAmount[],
  ) => void;
}

/**
 * Component that displays the frame for creating or updating an income transaction.
 */
const CreateOrUpdateIncomeTransactionFrame = function ({
  funds,
  amount,
  incomeFundAssignments,
  setIncomeFundAssignments,
}: CreateOrUpdateIncomeTransactionFrameProps): JSX.Element {
  return (
    <CaptionedFrame caption="Income Transaction Details">
      <FundAssignmentEntryFrame
        label="Fund Assignments"
        funds={funds}
        totalAmountToAssign={amount}
        value={incomeFundAssignments}
        setValue={setIncomeFundAssignments}
      />
    </CaptionedFrame>
  );
};

export default CreateOrUpdateIncomeTransactionFrame;
