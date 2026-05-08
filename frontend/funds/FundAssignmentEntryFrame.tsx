import "@/funds/FundAssignmentEntryFrame.css";
import { AddCircleOutline, Delete } from "@mui/icons-material";
import { Button, Stack, Tooltip, Typography } from "@mui/material";
import type { Fund, FundAmount } from "@/funds/types";
import { type JSX, useState } from "react";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import ColumnButton from "@/framework/listframe/ColumnButton";
import FundAmountEntryFrame from "@/funds/FundAmountEntryFrame";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Updates the amount assigned to the unassigned fund based on the total amount to assign and the amounts assigned to other funds.
 */
const updateUnassignedFundAmount = function (
  unassignedFund: Fund | null,
  totalAmountToAssign: number | null,
  fundAmounts: FundAmount[],
): FundAmount[] {
  const assignedFundAmounts = fundAmounts.filter(
    (fundAmount) => fundAmount.fundId !== unassignedFund?.id,
  );
  if (totalAmountToAssign === null || unassignedFund === null) {
    return assignedFundAmounts;
  }
  const assignedAmount = assignedFundAmounts.reduce(
    (acc, fundAmount) => acc + fundAmount.amount,
    0,
  );
  return [
    {
      fundId: unassignedFund.id,
      fundName: unassignedFund.name,
      amount: Math.max(totalAmountToAssign - assignedAmount, 0),
    },
    ...assignedFundAmounts,
  ];
};

/**
 * Props for the FundAssignmentEntryFrame component.
 */
interface FundAssignmentEntryFrameProps {
  readonly label: string;
  readonly funds: Fund[];
  readonly totalAmountToAssign: number | null;
  readonly value: FundAmount[];
  readonly setValue: (newValue: FundAmount[]) => void;
}

/**
 * Component that presents the user with an entry field where they can enter a Fund Amount.
 */
const FundAssignmentEntryFrame = function ({
  label,
  funds,
  totalAmountToAssign,
  value,
  setValue,
}: FundAssignmentEntryFrameProps): JSX.Element {
  const [focusIndex, setFocusIndex] = useState<number | null>(null);
  const unassignedFund =
    funds.find((fund) => fund.name === "Unassigned") ?? null;

  /**
   * Event handler for when the fund assignments are changed.
   */
  const onFundAssignmentChange = function (
    newFundAssignments: FundAmount[],
  ): void {
    const updatedFundAssignments = updateUnassignedFundAmount(
      unassignedFund,
      totalAmountToAssign,
      newFundAssignments,
    );
    setValue(updatedFundAssignments);
  };

  return (
    <div className="fund-assignment-entry-frame">
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
                setValue={
                  fundAmount.fundId === unassignedFund?.id
                    ? null
                    : (newValue): void => {
                        const newFundAmounts = [...value];
                        newFundAmounts[index] = newValue;
                        onFundAssignmentChange(newFundAmounts);
                      }
                }
                filter={(fund) =>
                  fund.id !== unassignedFund?.id &&
                  (value[index]?.fundId === fund.id ||
                    !value.some(
                      (existingFundAmount) =>
                        existingFundAmount.fundId === fund.id,
                    ))
                }
                autoFocus={focusIndex === index}
              />
              <ColumnButton
                label="Delete"
                icon={<Delete />}
                onClick={() => {
                  const newFundAmounts = value.filter((_, i) => i !== index);
                  onFundAssignmentChange(newFundAmounts);
                }}
                hidden={fundAmount.fundId === unassignedFund?.id}
              />
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
                onFundAssignmentChange(newFundAmounts);
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

export { updateUnassignedFundAmount };
export default FundAssignmentEntryFrame;
