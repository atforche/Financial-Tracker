import type { Fund, FundIdentifier } from "@/funds/types";
import { Box } from "@mui/material";
import FundEntryField from "@/funds/FundEntryField";
import type { JSX } from "react";
import TransactionSection from "@/transactions/workspace/TransactionSection";

interface TransactionFundPairSectionProps {
  readonly title: string;
  readonly description: string;
  readonly funds: Fund[];
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftFund: Fund | null;
  readonly rightFund: Fund | null;
  readonly setLeftFund: ((fund: Fund | null) => void) | null;
  readonly setRightFund: ((fund: Fund | null) => void) | null;
  readonly leftFilter?: ((fund: FundIdentifier) => boolean) | null;
  readonly rightFilter?: ((fund: FundIdentifier) => boolean) | null;
}

/**
 * Displays a pair of fund selectors for fund-to-fund transfer forms.
 */
const TransactionFundPairSection = function ({
  title,
  description,
  funds,
  leftLabel,
  rightLabel,
  leftFund,
  rightFund,
  setLeftFund,
  setRightFund,
  leftFilter = null,
  rightFilter = null,
}: TransactionFundPairSectionProps): JSX.Element {
  return (
    <TransactionSection title={title} description={description}>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 240px), 1fr))",
        }}
      >
        <FundEntryField
          label={leftLabel}
          options={funds}
          value={leftFund}
          setValue={
            setLeftFund === null
              ? null
              : (newValue): void => {
                  setLeftFund(
                    funds.find((fund) => fund.id === newValue?.id) ?? null,
                  );
                }
          }
          filter={leftFilter}
        />
        <FundEntryField
          label={rightLabel}
          options={funds}
          value={rightFund}
          setValue={
            setRightFund === null
              ? null
              : (newValue): void => {
                  setRightFund(
                    funds.find((fund) => fund.id === newValue?.id) ?? null,
                  );
                }
          }
          filter={rightFilter}
        />
      </Box>
    </TransactionSection>
  );
};

export default TransactionFundPairSection;
