import "@/framework/forms/FundAmountCollectionEntryFrame.css";
import { AddCircleOutline, Delete } from "@mui/icons-material";
import { Button, Stack, Tooltip, Typography } from "@mui/material";
import type { FundAmount, FundIdentifier } from "@/data/fundTypes";
import { type JSX, useState } from "react";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import ColumnButton from "@/framework/listframe/ColumnButton";
import FundAmountEntryFrame from "@/framework/forms/FundAmountEntryFrame";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the FundAmountCollectionEntryFrame component.
 */
interface FundAmountCollectionEntryFrameProps {
  readonly label: string;
  readonly funds: FundIdentifier[];
  readonly value: FundAmount[];
  readonly setValue: (newValue: FundAmount[]) => void;
  readonly lockedFundIds?: string[] | null;
}

/**
 * Component that presents the user with an entry field where they can enter a Fund Amount.
 * @param props - Props for the FundAmountCollectionEntryFrame component.
 * @returns JSX element representing the FundAmountCollectionEntryFrame component.
 */
const FundAmountCollectionEntryFrame = function ({
  label,
  funds,
  value,
  setValue,
  lockedFundIds = null,
}: FundAmountCollectionEntryFrameProps): JSX.Element {
  const [focusIndex, setFocusIndex] = useState<number | null>(null);
  return (
    <div className="fund-amount-collection-entry-frame">
      <CaptionedFrame caption={label}>
        <Stack spacing={2} sx={{ marginTop: "16px" }}>
          {value.map((fundAmount, index) => (
            <Stack
              key={index}
              direction="row"
              spacing={2}
              alignItems="center"
              justifyContent="space-between"
            >
              <FundAmountEntryFrame
                funds={funds}
                value={fundAmount}
                setValue={(newValue): void => {
                  const newFundAmounts = [...value];
                  newFundAmounts[index] = newValue;
                  setValue(newFundAmounts);
                }}
                filter={(fund) =>
                  value[index]?.fundId === fund.id ||
                  !value.some(
                    (existingFundAmount) =>
                      existingFundAmount.fundId === fund.id,
                  )
                }
                autoFocus={focusIndex === index}
                lockFund={lockedFundIds?.includes(fundAmount.fundId) ?? false}
              />
              {!(lockedFundIds?.includes(fundAmount.fundId) ?? false) && (
                <ColumnButton
                  label="Delete"
                  icon={<Delete />}
                  onClick={() => {
                    const newFundAmounts = value.filter((_, i) => i !== index);
                    setValue(newFundAmounts);
                  }}
                />
              )}
            </Stack>
          ))}

          <Tooltip title="Add Fund Amount">
            <Button
              variant="contained"
              startIcon={<AddCircleOutline />}
              disableElevation
              sx={{
                backgroundColor: "primary",
                border: 1,
                borderColor: "white",
              }}
              onClick={(): void => {
                const newFundAmounts = [
                  ...value,
                  { fundId: "", fundName: "", amount: 0 },
                ];
                setValue(newFundAmounts);
                setFocusIndex(newFundAmounts.length - 1);
              }}
            >
              Add Fund Amount
            </Button>
          </Tooltip>
          <Typography variant="body2">
            Total:{" "}
            {formatCurrency(
              value.reduce((acc, fundAmount) => acc + fundAmount.amount, 0),
            )}
          </Typography>
        </Stack>
      </CaptionedFrame>
    </div>
  );
};

export default FundAmountCollectionEntryFrame;
