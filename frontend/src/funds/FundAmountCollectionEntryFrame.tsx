import "@funds/FundAmountCollectionEntryFrame.css";
import { AddCircleOutline, Delete } from "@mui/icons-material";
import { type JSX, useState } from "react";
import { Stack, Typography } from "@mui/material";
import CaptionedFrame from "@framework/dialog/CaptionedFrame";
import ColumnButton from "@framework/listframe/ColumnButton";
import type { FundAmount } from "@funds/ApiTypes";
import FundAmountEntryFrame from "@funds/FundAmountEntryFrame";
import formatCurrency from "@framework/formatCurrency";

/**
 * Props for the FundAmountCollectionEntryFrame component.
 * @param label - Label for this Fund Amount Collection Entry Frame.
 * @param value - Current value for this Fund Amount Collection Entry Frame.
 * @param setValue - Callback to update the value in this Fund Amount Collection Entry Frame. If null, this field is read-only.
 */
interface FundAmountCollectionEntryFrameProps {
  readonly label: string;
  readonly value: FundAmount[];
  readonly setValue?: ((newValue: FundAmount[]) => void) | null;
}

/**
 * Component that presents the user with an entry field where they can enter a Fund Amount.
 * @param props - Props for the FundAmountCollectionEntryFrame component.
 * @returns JSX element representing the FundAmountCollectionEntryFrame component.
 */
const FundAmountCollectionEntryFrame = function ({
  label,
  value,
  setValue = null,
}: FundAmountCollectionEntryFrameProps): JSX.Element {
  const [newFundAmount, setNewFundAmount] = useState<FundAmount | null>(null);
  return (
    <div className="fund-amount-collection-entry-frame">
      <CaptionedFrame caption={label}>
        <Stack direction="row" spacing={2} alignItems="center">
          <FundAmountEntryFrame
            value={newFundAmount}
            setValue={setNewFundAmount}
            filter={(fund) =>
              !value.some((fundAmount) => fundAmount.fundId === fund.id)
            }
          />
          <ColumnButton
            label="Add Fund Amount"
            icon={<AddCircleOutline />}
            onClick={
              newFundAmount === null ||
              newFundAmount.fundId === "" ||
              newFundAmount.amount === 0
                ? null
                : (): void => {
                    if (setValue !== null) {
                      const newFundAmounts = [...value, newFundAmount];
                      setValue(newFundAmounts);
                      setNewFundAmount(null);
                    }
                  }
            }
          />
        </Stack>
        {value.map((fundAmount, index) => (
          <Stack
            key={fundAmount.fundId}
            direction="row"
            spacing={2}
            alignItems="center"
          >
            <FundAmountEntryFrame value={fundAmount} />
            <ColumnButton
              label="Delete"
              icon={<Delete />}
              onClick={() => {
                if (setValue !== null) {
                  const newFundAmounts = value.filter((_, i) => i !== index);
                  setValue(newFundAmounts);
                }
              }}
            />
          </Stack>
        ))}
        <Typography variant="body2">
          Total:{" "}
          {formatCurrency(
            value.reduce((acc, fundAmount) => acc + fundAmount.amount, 0),
          )}
        </Typography>
      </CaptionedFrame>
    </div>
  );
};

export default FundAmountCollectionEntryFrame;
