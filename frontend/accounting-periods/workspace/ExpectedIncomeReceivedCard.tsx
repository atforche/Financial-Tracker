import { Divider, Stack, Typography } from "@mui/material";
import type {
  ExpectedIncomeSource,
  ExpectedIncomeSourceIncome,
} from "@/accounting-periods/types";
import ContentSurface from "@/framework/view/ContentSurface";
import ExpectedIncomeLinkedIncome from "@/accounting-periods/workspace/ExpectedIncomeLinkedIncome";
import type { JSX } from "react";
import PeriodProgressRow from "@/accounting-periods/PeriodProgressRow";

/**
 * Props for the ExpectedIncomeReceivedCard component.
 */
interface ExpectedIncomeReceivedCardProps {
  readonly source: ExpectedIncomeSource;
  readonly income: readonly ExpectedIncomeSourceIncome[];
  readonly returnUrl: string;
}

/**
 * Compares the income received from an expected-income source against what it expects this period, and lists the
 * income linked to it.
 */
const ExpectedIncomeReceivedCard = function ({
  source,
  income,
  returnUrl,
}: ExpectedIncomeReceivedCardProps): JSX.Element {
  return (
    <ContentSurface>
      <Stack spacing={2}>
        <Typography variant="h5">Received</Typography>
        <PeriodProgressRow
          label="Tracked income"
          actual={source.receivedAmount.tracked}
          expected={source.expectedAmount.tracked}
          overLabel="Extra income"
        />
        {(source.expectedAmount.untracked !== 0 ||
          source.receivedAmount.untracked !== 0) && (
          <PeriodProgressRow
            label="Untracked income"
            actual={source.receivedAmount.untracked}
            expected={source.expectedAmount.untracked}
            overLabel="Extra income"
          />
        )}
        <Divider />
        <ExpectedIncomeLinkedIncome income={income} returnUrl={returnUrl} />
      </Stack>
    </ContentSurface>
  );
};

export default ExpectedIncomeReceivedCard;
