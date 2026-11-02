import { Chip, Divider, Stack, Typography } from "@mui/material";
import ContentSurface from "@/framework/view/ContentSurface";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import type { JSX } from "react";
import { PaymentLine } from "@/accounting-periods/workspace/ExpectedIncomePaymentCard";
import { formatShortDate } from "@/framework/dateHelpers";
import { getCurrencyTotal } from "@/framework/currencyHelpers";
import { getPaymentAmounts } from "@/accounting-periods/workspace/helpers";

/**
 * Props for the ExpectedIncomePeriodCard component.
 */
interface ExpectedIncomePeriodCardProps {
  readonly source: ExpectedIncomeSourceRequest;
}

/**
 * Shows the dates an expected-income source is expected to pay this period and what those payments add up to.
 */
const ExpectedIncomePeriodCard = function ({
  source,
}: ExpectedIncomePeriodCardProps): JSX.Element {
  const amounts = getPaymentAmounts(source);
  const paymentCount = source.expectedDates.length;
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h5">This period</Typography>
        <Stack spacing={1}>
          <Typography variant="body2" color="text.secondary">
            {paymentCount === 1
              ? "1 payment"
              : `${paymentCount.toString()} payments`}
          </Typography>
          {paymentCount > 0 && (
            <Stack
              direction="row"
              useFlexGap
              spacing={1}
              sx={{ flexWrap: "wrap" }}
            >
              {source.expectedDates.map((date) => (
                <Chip
                  key={date}
                  label={formatShortDate(new Date(`${date}T00:00:00`))}
                  size="small"
                  variant="outlined"
                />
              ))}
            </Stack>
          )}
        </Stack>
        <Divider />
        <Stack spacing={0.75}>
          <PaymentLine
            label="Expected"
            amount={getCurrencyTotal(
              source.expectedDates.map(() => amounts.net),
            )}
            total
          />
          {source.untrackedTransfers.length > 0 && (
            <PaymentLine
              label="Expected tracked"
              amount={getCurrencyTotal(
                source.expectedDates.map(() => amounts.tracked),
              )}
              total
            />
          )}
        </Stack>
      </Stack>
    </ContentSurface>
  );
};

export default ExpectedIncomePeriodCard;
