import { Button, Stack, Typography } from "@mui/material";
import type { AccountingPeriodFund } from "@/data/fundTypes";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import CaptionedValue from "@/framework/view/CaptionedValue";
import type { JSX } from "react";
import formatCurrency from "@/framework/formatCurrency";

/**
 * Props for the CurrentGoalFrame component.
 */
interface CurrentGoalFrameProps {
  readonly fund: AccountingPeriodFund;
  readonly accountingPeriodId: string;
  readonly isAccountingPeriodOpen: boolean;
}

/**
 * Component that displays the current goal for a fund and accounting period.
 */
const CurrentGoalFrame = function ({
  fund,
  accountingPeriodId,
  isAccountingPeriodOpen,
}: CurrentGoalFrameProps): JSX.Element {
  return (
    <CaptionedFrame caption="Current Goal" maxWidth={null}>
      <Stack
        direction="row"
        spacing={2}
        justifyContent="space-between"
        alignItems="center"
        sx={{ width: 1000 }}
      >
        {fund.goalAmount !== null ? (
          <Stack width={500}>
            <CaptionedValue caption="Goal Type" value={fund.goalType ?? "-"} />
            <CaptionedValue
              caption="Goal Amount"
              value={formatCurrency(fund.goalAmount)}
            />
          </Stack>
        ) : (
          <Typography variant="subtitle1">No current goal.</Typography>
        )}
        {fund.goalAmount !== null ? (
          <Stack direction="row" spacing={1}>
            <Button
              variant="contained"
              color="primary"
              href={`/accounting-periods/${accountingPeriodId}/funds/${fund.id}/goal/update`}
              disabled={fund.isSystemFund || !isAccountingPeriodOpen}
            >
              Edit
            </Button>
            <Button
              variant="contained"
              color="error"
              href={`/accounting-periods/${accountingPeriodId}/funds/${fund.id}/goal/delete`}
              disabled={fund.isSystemFund || !isAccountingPeriodOpen}
            >
              Delete
            </Button>
          </Stack>
        ) : (
          <Button
            variant="contained"
            color="primary"
            href={`/accounting-periods/${accountingPeriodId}/funds/${fund.id}/goal/create`}
            disabled={fund.isSystemFund || !isAccountingPeriodOpen}
          >
            Add
          </Button>
        )}
      </Stack>
      {fund.goalAmount !== null && (
        <>
          <CaptionedValue
            caption="Previous Month Balance"
            value={formatCurrency(fund.openingBalance.postedBalance)}
          />
          <CaptionedValue
            caption="Amount Assigned"
            value={
              <span
                style={{
                  color:
                    fund.goalAmount - fund.openingBalance.postedBalance >= 0
                      ? "green"
                      : "red",
                }}
              >
                {fund.goalAmount - fund.openingBalance.postedBalance >= 0
                  ? "+"
                  : "-"}{" "}
                {formatCurrency(
                  Math.abs(fund.goalAmount - fund.openingBalance.postedBalance),
                )}
              </span>
            }
          />
          <CaptionedValue
            caption="Amount Spent"
            value={
              <span style={{ color: "red" }}>
                -{formatCurrency(fund.amountSpent)}
              </span>
            }
          />
          <CaptionedValue
            caption="Amount Remaining"
            value={
              <span
                style={{
                  color:
                    fund.goalAmount -
                      fund.openingBalance.postedBalance -
                      fund.amountSpent >=
                    0
                      ? "green"
                      : "red",
                }}
              >
                {fund.goalAmount -
                  fund.openingBalance.postedBalance -
                  fund.amountSpent >=
                0
                  ? "+"
                  : "-"}{" "}
                {formatCurrency(
                  Math.abs(
                    fund.goalAmount -
                      fund.openingBalance.postedBalance -
                      fund.amountSpent,
                  ),
                )}
              </span>
            }
          />
        </>
      )}
    </CaptionedFrame>
  );
};

export default CurrentGoalFrame;
