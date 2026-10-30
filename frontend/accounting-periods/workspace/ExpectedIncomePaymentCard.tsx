import { Divider, Stack, Typography } from "@mui/material";
import { formatCurrency, getCurrencyTotal } from "@/framework/currencyHelpers";
import ContentSurface from "@/framework/view/ContentSurface";
import type { ExpectedIncomeSourceRequest } from "@/accounting-periods/types";
import type { JSX } from "react";
import { getPaymentAmounts } from "@/accounting-periods/workspace/helpers";

/**
 * Props for the ExpectedIncomePaymentCard component.
 */
interface ExpectedIncomePaymentCardProps {
  readonly source: ExpectedIncomeSourceRequest;
}

/**
 * Props for the PaymentLine component.
 */
interface PaymentLineProps {
  readonly label: string;
  readonly amount: number;
  readonly total?: boolean;
  readonly indent?: boolean;
}

/**
 * Displays one line of an amount breakdown. Totals are bold, and lines within a group are indented.
 */
const PaymentLine = function ({
  label,
  amount,
  total = false,
  indent = false,
}: PaymentLineProps): JSX.Element {
  return (
    <Stack
      direction="row"
      spacing={2}
      sx={{ justifyContent: "space-between", pl: indent ? 2 : 0 }}
    >
      <Typography
        variant="body2"
        noWrap
        {...(total ? { fontWeight: 600 } : { color: "text.secondary" })}
      >
        {label}
      </Typography>
      <Typography variant="body2" {...(total ? { fontWeight: 600 } : {})}>
        {formatCurrency(amount)}
      </Typography>
    </Stack>
  );
};

/**
 * Props for the PaymentGroup component.
 */
interface PaymentGroupProps {
  readonly label: string;
  readonly lines: readonly { description: string; amount: number }[];
  readonly sign: 1 | -1;
}

/**
 * Displays a group of lines in a payment's breakdown under a heading that carries the group's subtotal, or nothing
 * when the group is empty.
 */
const PaymentGroup = function ({
  label,
  lines,
  sign,
}: PaymentGroupProps): JSX.Element | null {
  if (lines.length === 0) {
    return null;
  }
  return (
    <Stack spacing={0.75}>
      <Stack
        direction="row"
        spacing={2}
        sx={{ justifyContent: "space-between" }}
      >
        <Typography variant="subtitle2">{label}</Typography>
        <Typography variant="subtitle2">
          {formatCurrency(
            sign * getCurrencyTotal(lines.map((line) => line.amount)),
          )}
        </Typography>
      </Stack>
      {lines.map((line, index) => (
        <PaymentLine
          key={`${label}-${index.toString()}`}
          label={line.description}
          amount={sign * line.amount}
          indent
        />
      ))}
    </Stack>
  );
};

/**
 * Breaks one expected payment down from its income lines to the amount that reaches tracked accounts.
 */
const ExpectedIncomePaymentCard = function ({
  source,
}: ExpectedIncomePaymentCardProps): JSX.Element {
  const amounts = getPaymentAmounts(source);
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h5">Each payment</Typography>
        <Stack spacing={1.5}>
          <PaymentGroup
            label="Income lines"
            lines={source.incomeLines}
            sign={1}
          />
          <PaymentGroup
            label="Deductions"
            lines={source.incomeDeductions}
            sign={-1}
          />
          <Divider />
          <PaymentLine label="Net" amount={amounts.net} total />
          <PaymentGroup
            label="Untracked transfers"
            lines={source.untrackedTransfers}
            sign={-1}
          />
          {source.untrackedTransfers.length > 0 && (
            <>
              <Divider />
              <PaymentLine label="Tracked" amount={amounts.tracked} total />
            </>
          )}
        </Stack>
      </Stack>
    </ContentSurface>
  );
};

export { PaymentLine };
export default ExpectedIncomePaymentCard;
